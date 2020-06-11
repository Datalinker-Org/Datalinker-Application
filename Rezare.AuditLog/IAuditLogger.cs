using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog
{
    /// <summary>
    /// The <see cref="IAuditLogger"/> interface is used by the application to log audit event messages into the Audit Logger framework.
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Logs the given data parameters to the given audit stream.
        /// </summary>
        /// <param name="auditStream">Stream for the audit event.</param>
        /// <param name="action">Summary of the action taken.</param>
        /// <param name="userDetails">Details to identify the user taking the action.</param>
        /// <param name="dataPayload">Details about the action taken.</param>
        void Log(AuditStream auditStream, string action, object userDetails, object dataPayload);
    }
}
