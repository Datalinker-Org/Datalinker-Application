using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezare.AuditLog.Database
{
    public static class SqlConnectionExtensions
    {
        /// <summary>
        /// Ensures that the <see cref="SqlConnection"/> is in a state to run database
        /// queries.
        /// </summary>
        /// <param name="connection"><see cref="SqlConnection"/> to extend.</param>
        public static void EnsureOpen(this SqlConnection connection)
        {
            lock(connection)
            {
                if (connection.State == ConnectionState.Broken)
                {
                    connection.Close();
                }

                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
            }
        }
    }
}
