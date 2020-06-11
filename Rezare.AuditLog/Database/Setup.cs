using System;
using System.Data.SqlClient;
using System.IO;

namespace Rezare.AuditLog.Database
{
    /// <summary>
    /// Class to check and update the audit logging database, if required.
    /// </summary>
    internal class Setup
    {
        private readonly string _connectionString;

        /// <summary>
        /// Instantiates the <see cref="Setup"/> class with the connection string for
        /// that database to check and update, if required.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        public Setup(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Ensures the tables are at the latest version and that all required
        /// SEQUENCE objects have been created in the database.
        /// </summary>
        public void CheckAndUpgradeDatabase()
        {
            UpgradeTables();
            CreateMissingSequences();

            ReportDatabaseVersion();
        }

        /// <summary>
        /// Reports the current version number and upgrade date to the log file.
        /// Throws an <see cref="InvalidOperationException"/> if they cannot be
        /// read from the database.
        /// </summary>
        private void ReportDatabaseVersion()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT TOP 1 [VersionNumber], [UpgradedAt] FROM [dbo].[VersionInformation] ORDER BY [VersionNumber] DESC";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Unable to fetch version information from the database [server={0}, database={1}].",
                            connection.DataSource, connection.Database));
                    }
                }
            }
        }

        /// <summary>
        /// Runs the Tables.sql script to perform any necessary table updates.
        /// </summary>
        private void UpgradeTables()
        {
            var tablesScript = LoadEmbeddedSqlFile("Tables.sql");

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = tablesScript;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates any missing SEQUENCE objects in the database. The SQL file is a template
        /// and the actual required sequence objects are determined by the values of
        /// <see cref="AuditStream"/>.
        /// </summary>
        private void CreateMissingSequences()
        {
            var sequenceScript = LoadEmbeddedSqlFile("SequenceTemplate.sql");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var sequence in Enum.GetValues(typeof(AuditStream)))
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = string.Format(sequenceScript, sequence);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Load the desired embedded SQL script.
        /// </summary>
        /// <param name="name">Name of the embedded SQL resource to load, without namespace.</param>
        /// <returns>SQL script contents.</returns>
        private string LoadEmbeddedSqlFile(string name)
        {
            var assembly = typeof(IAuditLogger).Assembly;
            var resourceName = $"Rezare.AuditLog.Database.Sql.{name}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Requested resource {resourceName} not found in {assembly}.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
