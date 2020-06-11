using Rezare.AuditLog.Crypto;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Rezare.AuditLog.Config
{
    public class AuditLogConfiguration
    {
        /// <summary>
        /// Loads the configuration from the application's .config file.
        /// </summary>
        public AuditLogConfiguration()
        {
            XmlElement configElement = ConfigurationManager.GetSection("auditlog") as XmlElement;

            if (configElement == null)
            {
                throw new ConfigurationErrorsException("Failed to find configuration section 'auditlog' in the application's .config file.");
            }
            else
            {
                SetProperties(configElement);
            }
        }

        /// <summary>
        /// Extract and store the configuration settings from the <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="configElement"><see cref="XmlElement"/> with configuration values from the application's .config file.</param>
        private void SetProperties(XmlElement configElement)
        {
            IsAuditLogEnabled = bool.Parse(configElement["enabled"].InnerText ?? "True");

            ConnectionString = configElement["connectionString"].InnerText;

            IsX509StoreValid = true;
            try
            {
                X509StoreName = (StoreName)Enum.Parse(typeof(StoreName), configElement["x509StoreName"].InnerText);
                X509StoreLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), configElement["x509StoreLocation"].InnerText);
            }
            catch (ArgumentException)
            {
                IsX509StoreValid = false;
            }

            X509CertificateSubject = configElement["x509CertificateSubject"].InnerText;
            if (string.IsNullOrWhiteSpace(X509CertificateSubject))
            {
                IsX509StoreValid = false;
            }

            X509CertificateFilePath = configElement["x509CertificateFilePath"].InnerText;
            IsX509FileValid = !string.IsNullOrWhiteSpace(X509CertificateFilePath) && File.Exists(X509CertificateFilePath);

            try
            {
                HashAlgorithm = (SupportedHashAlgorithm)Enum.Parse(typeof(SupportedHashAlgorithm), configElement["hashAlgorithm"].InnerText);
            }
            catch (ArgumentException)
            {
                HashAlgorithm = SupportedHashAlgorithm.SHA1;
            }
        }

        /// <summary>
        /// Gets a flag indicating if Audit Logging is enabled or not.
        /// </summary>
        public bool IsAuditLogEnabled { get; private set; }

        /// <summary>
        /// Gets the database conection string for the audit logging database.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the X.509 certificate store name.
        /// </summary>
        public StoreName X509StoreName { get; private set; }

        /// <summary>
        /// Gets the X.509 certificate store location.
        /// </summary>
        public StoreLocation X509StoreLocation { get; private set; }

        /// <summary>
        /// Gets the X.509 certificate subject.
        /// </summary>
        public string X509CertificateSubject { get; private set; }

        /// <summary>
        /// Indicates if the X.509 store settings are valid.
        /// </summary>
        public bool IsX509StoreValid { get; private set; }

        /// <summary>
        /// Gets the file path to a X.509 certificate.
        /// </summary>
        public string X509CertificateFilePath { get; private set; }

        /// <summary>
        /// Indicates if the X.509 certificate path has been set and the file exists.
        /// </summary>
        public bool IsX509FileValid { get; private set; }

        /// <summary>
        /// Gets the name of the hashing / has signing algorithm to use.
        /// </summary>
        public SupportedHashAlgorithm HashAlgorithm { get; private set; }

        /// <summary>
        /// Returns a string that represents this <see cref="AuditLogConfiguration"/> object.
        /// </summary>
        /// <returns>A string that represents this <see cref="AuditLogConfiguration"/> object.</returns>
        public override string ToString()
        {
            return string.Format("enabled={7}, connection_string=\"{0}\", store_name={1}, store_location={2}, certificate_subject={3}, is_store_valid={4}, certificate_file_path={5}, is_file_valid={6}, hash_algorithm={8}",
                ConnectionString, X509StoreName, X509StoreLocation, X509CertificateSubject, IsX509StoreValid, X509CertificateFilePath, IsX509FileValid, IsAuditLogEnabled, HashAlgorithm);
        }
    }
}
