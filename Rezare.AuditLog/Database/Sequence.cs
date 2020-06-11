using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog.Database
{
    /// <summary>
    /// Class for getting message sequence numbers.
    /// </summary>
    internal static class Sequence
    {        
        /// <summary>
        /// Returns the next number in the sequence for the given stream.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="auditStream">Audit stream to get the next sequence number for.</param>
        /// <returns>Sequence number.</returns>
        public static long GetNext(SqlConnection connection, AuditStream auditStream)
        {
            connection.EnsureOpen();
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = $"SELECT NEXT VALUE FOR dbo.{auditStream}";

                return (long) command.ExecuteScalar();
            }
        }
    }
}
