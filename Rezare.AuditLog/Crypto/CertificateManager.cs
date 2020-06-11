using Rezare.AuditLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog.Certificates
{
    /// <summary>
    /// Provides the private/public keys used for signing and verifying signatures. Certificates
    /// will be loaded from the built in certificate store, or the filesystem. If both locations
    /// are configured then only the certificate store will be considered.
    /// </summary>
    internal class CertificateManager
    {
        private readonly AuditLogConfiguration _configuration;

        private RSACryptoServiceProvider _signingPcs = null;
        private RSACryptoServiceProvider _verificationPcs = null;

        /// <summary>
        /// Set up the certificate manager.
        /// </summary>
        /// <param name="configuration"><see cref="AuditLogConfiguration"/> with certificate location details.</param>
        public CertificateManager(AuditLogConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Returns the <see cref="RSACryptoServiceProvider"/> to be used for signing. It contains the private key from
        /// the configured X.509 certificate.
        /// </summary>
        /// <returns><see cref="RSACryptoServiceProvider"/> for signing.</returns>
        public RSACryptoServiceProvider GetSigningProvider()
        {
            var certFound = false;
            if (_signingPcs == null)
            {
                if (_configuration.IsX509StoreValid)
                {
                    X509Store store = new X509Store(_configuration.X509StoreName, _configuration.X509StoreLocation);
                    store.Open(OpenFlags.ReadOnly);
                    var certs = new List<string>();
                    foreach (var certificate in store.Certificates)
                    {
                        certs.Add(certificate.Subject);
                        if (certificate.Subject.Contains(_configuration.X509CertificateSubject))
                        {
                            certFound = true;
                            try
                            {
                                _signingPcs = (RSACryptoServiceProvider)certificate.PrivateKey;
                            }
                            catch(Exception ex)
                            {
                                throw new Exception("Unable to access private key", ex);
                            }
                        }
                    }

                    store.Close();
                    if (!certFound)
                    {
                        throw new Exception($"Certificate with subject '{_configuration.X509CertificateSubject}' is found '{certFound}' in store '{store.Name}'. Available certs: {string.Join(",", certs)}");
                    }
                }
                else if (_configuration.IsX509FileValid)
                {
                    var certificate = new X509Certificate2(_configuration.X509CertificateFilePath);
                    _signingPcs = (RSACryptoServiceProvider) certificate.PrivateKey;
                }
                else
                {
                    throw new InvalidOperationException("IsX509StoreValid and IsX509FileValid are both false!");
                }
            }

            return _signingPcs;
        }

        /// <summary>
        /// Returns the <see cref="RSACryptoServiceProvider"/> to be used for verifying signatures. It contains the
        /// public key from the configured X.509 certificate.
        /// </summary>
        /// <returns><see cref="RSACryptoServiceProvider"/> for signature varification.</returns>
        public RSACryptoServiceProvider GetVerificationProvider()
        {
            if (_verificationPcs == null)
            {
                if (_configuration.IsX509StoreValid)
                {
                    X509Store store = new X509Store(_configuration.X509StoreName, _configuration.X509StoreLocation);
                    store.Open(OpenFlags.ReadOnly);

                    foreach (var certificate in store.Certificates)
                    {
                        if (certificate.Subject.Contains(_configuration.X509CertificateSubject))
                        {
                            _verificationPcs = (RSACryptoServiceProvider)certificate.PublicKey.Key;
                        }
                    }
                }
                else if (_configuration.IsX509FileValid)
                {
                    var certificate = new X509Certificate2(_configuration.X509CertificateFilePath);
                    _verificationPcs = (RSACryptoServiceProvider)certificate.PublicKey.Key;
                }
                else
                {
                    throw new InvalidOperationException("IsX509StoreValid and IsX509FileValid are both false!");
                }
            }

            return _verificationPcs;
        }

        /// <summary>
        /// Forces a certificate reload when <see cref="GetSigningProvider"/> or <see cref="GetVerificationProvider"/>
        /// are called.
        /// </summary>
        public void RefreshCachedKeys()
        {
            _signingPcs = null;
            _verificationPcs = null;
        }
    }
}
