using Rezare.AuditLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog
{
    /// <summary>
    /// Exception that is thrown when the audit log encounters an issue. The exception will contain an inner exception
    /// and the audit event entry if they are available.
    /// </summary>
    public class AuditEventWriteException : Exception
    {
        /// <summary>
        /// Gets the <see cref="Models.AuditEntry"/> details for this exception, or null if there are not any.
        /// </summary>
        public AuditEntry AuditEntry { get; private set; }

        /// <summary>
        /// Instantiate the <see cref="AuditEventWriteException"/> with a message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public AuditEventWriteException(string message) : base(message)
        {
            AuditEntry = null;
        }

        /// <summary>
        /// Instantiate the <see cref="AuditEventWriteException"/> with a message and <see cref="Models.AuditEntry"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="auditEntry">Associated <see cref="Models.AuditEntry"/>.</param>
        public AuditEventWriteException(string message, AuditEntry auditEntry) : base(message)
        {
            AuditEntry = auditEntry;
        }

        /// <summary>
        /// Instantiate the <see cref="AuditEventWriteException"/> with a message and inner exception.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerExeption">Inner exception.</param>
        public AuditEventWriteException(string message, Exception innerExeption) : base(message, innerExeption)
        {
            AuditEntry = null;
        }
        
        /// <summary>
        /// Instantiate the <see cref="AuditEventWriteException"/> with a message, <see cref="Models.AuditEntry"/> and
        /// inner exception.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="auditEntry">Associated <see cref="Models.AuditEntry"/>.</param>
        /// <param name="innerExeption">Inner exception.</param>
        public AuditEventWriteException(string message, AuditEntry auditEntry, Exception innerExeption) : base(message, innerExeption)
        {
            AuditEntry = auditEntry;
        }
    }
}
