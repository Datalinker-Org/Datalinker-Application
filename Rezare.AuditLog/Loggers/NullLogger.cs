using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog.Loggers
{
    /// <summary>
    /// This logger does nothing. This is the logger that is returned by the
    /// <see cref="AuditLogManager"/> when audit logging has been disabled.
    /// </summary>
    internal class NullLogger : IAuditLogger
    {
        /// <summary>
        /// Logging method that does nothing.
        /// </summary>
        /// <param name="auditStream">Ignored audit stream.</param>
        /// <param name="action">Ignored user action.</param>
        /// <param name="userDetails">Ignored user details.</param>
        /// <param name="dataPayload">Ignored data payload.</param>
        public void Log(AuditStream auditStream, string action, object userDetails, object dataPayload)
        {
            // This is intentionally empty
        }
    }
}
