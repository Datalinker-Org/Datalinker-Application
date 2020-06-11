using Newtonsoft.Json;
using Rezare.AuditLog.Certificates;
using Rezare.AuditLog.Config;
using Rezare.AuditLog.Database;
using Rezare.AuditLog.Models;
using System;
using System.Data.SqlClient;

namespace Rezare.AuditLog.Loggers
{
    /// <summary>
    /// Implementation of an <see cref="IAuditLogger"/> that writes audit log messages
    /// to a database table.
    /// </summary>
    internal class AuditDatabaseLogger : IAuditLogger
    {
        private readonly AuditLogConfiguration _configuration;
        private readonly CertificateManager _certificateManager;
        private DateTime GetDate => DateTime.UtcNow;

        public AuditDatabaseLogger(AuditLogConfiguration configuration, CertificateManager certificateManager)
        {
            _configuration = configuration;
            _certificateManager = certificateManager;
            // Check and update the database now
            var dbSetup = new Setup(_configuration.ConnectionString);
            dbSetup.CheckAndUpgradeDatabase();
        }

        /// <summary>
        /// Logs the given data parameters to the given audit stream.
        /// </summary>
        /// <param name="auditStream">Stream for the audit event.</param>
        /// <param name="action">Summary of the action taken.</param>
        /// <param name="userDetails">Details to identify the user taking the action.</param>
        /// <param name="dataPayload">Details about the action taken.</param>
        public void Log(AuditStream auditStream, string action, object userDetails, object dataPayload)
        {
            using (var connection = new SqlConnection(_configuration.ConnectionString))
            {
                connection.Open();
                var auditEntry = new AuditEntry()
                {
                    Sequence = Sequence.GetNext(connection, auditStream),
                    CreatedAt = GetDate,
                    Stream = auditStream.ToString(),
                    Action = action,
                    UserDetails = JsonConvert.SerializeObject(userDetails, Formatting.Indented),
                    DataPayload = JsonConvert.SerializeObject(dataPayload, Formatting.Indented)
                };
                var signingProvider = _certificateManager.GetSigningProvider();
                auditEntry.SignAuditMesssage(signingProvider, _configuration.HashAlgorithm);
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    command.CommandText = @"
INSERT INTO [AuditLogMessages]
    ([AuditStream],[Sequence],[CreatedAt],[Action],[UserDetails],[DataPayload],[Signature])
VALUES
    (@AuditStream,@Sequence,@CreatedAt,@Action,@UserDetails,@DataPayload,@Signature)";
                    command.Parameters.Add(new SqlParameter("@AuditStream", auditEntry.Stream));
                    command.Parameters.Add(new SqlParameter("@Sequence", auditEntry.Sequence));
                    command.Parameters.Add(new SqlParameter("@CreatedAt", auditEntry.CreatedAt));
                    command.Parameters.Add(new SqlParameter("@Action", auditEntry.Action));
                    command.Parameters.Add(new SqlParameter("@UserDetails", auditEntry.UserDetails));
                    command.Parameters.Add(new SqlParameter("@DataPayload", auditEntry.DataPayload));
                    command.Parameters.Add(new SqlParameter("@Signature", auditEntry.Signature));

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlex)
                    {
                        throw new AuditEventWriteException("Failed to write audit log entry", auditEntry, sqlex);
                    }
                }
            }
        }
    }
}
