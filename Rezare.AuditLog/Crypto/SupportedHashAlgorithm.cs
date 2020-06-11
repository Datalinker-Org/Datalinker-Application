using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog.Crypto
{
    /// <summary>
    /// Hash algorithms that are supported by the Audit Log framework.
    /// </summary>
    public enum SupportedHashAlgorithm
    {
        /// <summary>
        /// SHA-1 hash algorithm.
        /// </summary>
        SHA1,

        /// <summary>
        /// SHA-256 hash algorithm.
        /// </summary>
        SHA256,

        /// <summary>
        /// SHA-384 hash algorithm.
        /// </summary>
        SHA384,

        /// <summary>
        /// SHA-512 hash algorithm.
        /// </summary>
        SHA512
    }
}
