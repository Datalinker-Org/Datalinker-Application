using Rezare.AuditLog.Certificates;
using Rezare.AuditLog.Config;
using Rezare.AuditLog.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog
{
    /// <summary>
    /// Factory that provides the <see cref="IAuditLogger"/> for audit logging.
    /// </summary>
    public static class AuditLogManager
    {
        private static IAuditLogger _auditLogger = null;
        private static readonly AuditLogConfiguration _configuration;

        /// <summary>
        /// Static constructor that loads the configuration for the Audit Logging
        /// framework.
        /// </summary>
        static AuditLogManager()
        {
            _configuration = new AuditLogConfiguration();
        }

        /// <summary>
        /// Returns an <see cref="IAuditLogger"/> for audit logging.
        /// </summary>
        /// <returns><see cref="IAuditLogger"/> for audit logging.</returns>
        public static IAuditLogger GetAuditLogger()
        {
            if (_auditLogger == null)
            {
                if (_configuration.IsAuditLogEnabled)
                {
                    _auditLogger = new AuditDatabaseLogger(_configuration, new CertificateManager(_configuration));
                }
                else
                {
                    _auditLogger = new NullLogger();
                }
            }

            return _auditLogger;
        }
    }
}
