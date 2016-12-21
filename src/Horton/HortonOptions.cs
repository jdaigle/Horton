using System;
using System.Data.SqlClient;
using System.IO;

namespace Horton
{
    public class HortonOptions
    {
        public HortonCommand Command { get; set; }
        public string[] ExtraArguments { get; set; }

        public string MigrationsDirectoryPath { get; set; } = Environment.CurrentDirectory;
        public string ServerHostname { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "";

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConnectionString { get; set; } = "";

        public bool Unattend { get; set; } = false;

        public bool IsIntegratedSecurity => string.IsNullOrWhiteSpace(Username);

        public string CreateConnectionString()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = ServerHostname;
            connectionStringBuilder.InitialCatalog = DatabaseName;
            connectionStringBuilder.IntegratedSecurity = IsIntegratedSecurity;
            if (!IsIntegratedSecurity)
            {
                connectionStringBuilder.UserID = Username;
                connectionStringBuilder.Password = Password;
            }
            connectionStringBuilder.Pooling = false;
            return connectionStringBuilder.ToString();
        }

        public bool AssertValid(out string firstValidationMessage)
        {
            if (Command == null)
            {
                firstValidationMessage = "Command is required.";
                return false;
            }

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
                ServerHostname = connectionStringBuilder.DataSource;
                DatabaseName = connectionStringBuilder.InitialCatalog;
                Username = connectionStringBuilder.UserID;
                Password = connectionStringBuilder.Password;
            }

            TryGetDatabaseNameFromFile();
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                firstValidationMessage = "Database name is required (either by parameter or \"database.name\" file).";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Password))
            {
                firstValidationMessage = "Password is required when Username is provided.";
                return false;
            }

            firstValidationMessage = "";
            return true;
        }

        private void TryGetDatabaseNameFromFile()
        {
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                var databaseNameFile = Path.Combine(MigrationsDirectoryPath, "database.name");
                if (File.Exists(databaseNameFile))
                {
                    var lines = File.ReadAllLines(databaseNameFile);
                    if (lines.Length > 0)
                    {
                        DatabaseName = lines[0].Trim();
                    }
                }
            }
        }
    }
}
