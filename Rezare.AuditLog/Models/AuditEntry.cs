using Rezare.AuditLog.Crypto;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rezare.AuditLog.Models
{
    /// <summary>
    /// Object to encapsulate an audit event message.
    /// </summary>
    public class AuditEntry
    {
        /// <summary>
        /// Gets or sets the stream this audit message entry belongs to. Valid
        /// values for this are in <see cref="AuditStream"/>.
        /// </summary>
        public string Stream { get; set; }

        /// <summary>
        /// Gets or sets the message sequence number. This is a serialially incrementing
        /// number grouped by <see cref="Stream"/>.
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// Gets or sets the date and time this message was created. Time is stored as UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the action summary for the message.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the JSON encoded user details object.
        /// </summary>
        public string UserDetails { get; set; }

        /// <summary>
        /// Gets or sets the JSON encoded data payload object.
        /// </summary>
        public string DataPayload { get; set; }

        /// <summary>
        /// Gets the Base64 encoded SHA256 hash for this <see cref="AuditEntry"/> object. This
        /// field is only populated after calling <see cref="SignAuditMesssage(RSACryptoServiceProvider)"/> or
        /// <see cref="VerifyAuditMessage(RSACryptoServiceProvider)"/>.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Gets the Base64 coded cryptographic signature for this <see cref="AuditEntry"/> object.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Uses the provided <see cref="RSACryptoServiceProvider"/> to sign this <see cref="AuditEntry"/>.
        /// </summary>
        /// <param name="csp"><see cref="RSACryptoServiceProvider"/> that contains a private key to sign this <see cref="AuditEntry"/> with.</param>
        /// <param name="hashAlgorithm"><see cref="SupportedHashAlgorithm"/> to use for signing data.</param>
        public void SignAuditMesssage(RSACryptoServiceProvider csp, SupportedHashAlgorithm hashAlgorithm)
        {
            if (Signature != null)
            {
                throw new InvalidOperationException("Signature already set!");
            }

            var hash = HashMessage(hashAlgorithm);
            var algorithm = CryptoConfig.MapNameToOID(hashAlgorithm.ToString());
            var signature = csp.SignHash(hash, algorithm);

            Hash = Convert.ToBase64String(hash);
            Signature = Convert.ToBase64String(signature);
        }

        /// <summary>
        /// Uses the provided <see cref="RSACryptoServiceProvider"/> to verify the <see cref="Signature"/>
        /// for this <see cref="AuditEntry"/>.
        /// </summary>
        /// <param name="csp"><see cref="RSACryptoServiceProvider"/> that contains a public key to verify this <see cref="AuditEntry"/> with.</param>
        /// <param name="hashAlgorithm"><see cref="SupportedHashAlgorithm"/> to use for verifying signature.</param>
        /// <returns>True if the signature is valid, false otherwise.</returns>
        public bool VerifyAuditMessage(RSACryptoServiceProvider csp, SupportedHashAlgorithm hashAlgorithm)
        {
            var hash = HashMessage(hashAlgorithm);

            Hash = Convert.ToBase64String(hash);
            return csp.VerifyHash(hash, CryptoConfig.MapNameToOID(hashAlgorithm.ToString()), Convert.FromBase64String(Signature));
        }

        /// <summary>
        /// Builds a SHA256 hash of the data contained in this <see cref="AuditEntry"/>.
        /// </summary>
        /// <param name="hashAlgorithm"><see cref="SupportedHashAlgorithm"/> to use for hashing data.</param>
        /// <returns>Hash byte array.</returns>
        private byte[] HashMessage(SupportedHashAlgorithm hashAlgorithm)
        {
            HashAlgorithm hasher = null;
            switch(hashAlgorithm)
            {
                case SupportedHashAlgorithm.SHA1:
                    hasher = new SHA1Managed();
                    break;
                case SupportedHashAlgorithm.SHA256:
                    hasher = new SHA256Managed();
                    break;
                case SupportedHashAlgorithm.SHA384:
                    hasher = new SHA384Managed();
                    break;
                case SupportedHashAlgorithm.SHA512:
                    hasher = new SHA512Managed();
                    break;
            }
            
            var endoding = new UnicodeEncoding();

            var id = BitConverter.GetBytes(Sequence);
            var createdAt = BitConverter.GetBytes(CreatedAt.ToBinary());
            var strData = endoding.GetBytes($"{Stream}{Action}{UserDetails}{DataPayload}");

            return hasher.ComputeHash(id.Concat(createdAt).Concat(strData).ToArray());
        }

        /// <summary>
        /// Returns a short string that represents this <see cref="AuditEntry"/> object.
        /// </summary>
        /// <returns>A short string that represents this <see cref="AuditEntry"/> object.</returns>
        public string ToShortString()
        {
            return string.Format("stream={0}, sequence={1}, hash={2}",
                Stream, Sequence, Hash);
        }

        /// <summary>
        /// Returns a string that represents this <see cref="AuditEntry"/> object.
        /// </summary>
        /// <returns>A string that represents this <see cref="AuditEntry"/> object.</returns>
        public override string ToString()
        {
            return string.Format("stream={0}, sequence={1}, created_at={2}, action={3}, user_details={4}, data_payload={5}, signature={6}",
                Stream, Sequence, CreatedAt, Action, UserDetails, DataPayload, Signature);
        }
    }
}
