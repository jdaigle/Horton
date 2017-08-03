using System;
using System.Data.SqlClient;

namespace Horton
{
    public class HortonOptions
    {
        public string FilesPath { get; set; } = Environment.CurrentDirectory;

        public string DatabaseName { get; set; } = "";

        public string ServerHostname { get; set; } = "(local)";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsIntegratedSecurity => string.IsNullOrWhiteSpace(Username);
        public string ConnectionString { get; set; } = "";

        public bool Unattend { get; set; } = false;

        public bool DryRun { get; set; } = false;

        public bool WarnAndRerunModifiedMigrations { get; set; } = false;

        public bool RunBaseline { get; set; } = false;

        public string CreateConnectionString()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = ServerHostname,
                InitialCatalog = DatabaseName,
                IntegratedSecurity = IsIntegratedSecurity
            };
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
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
                ServerHostname = connectionStringBuilder.DataSource;
                DatabaseName = connectionStringBuilder.InitialCatalog;
                Username = connectionStringBuilder.UserID;
                Password = connectionStringBuilder.Password;
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                firstValidationMessage = "Database name is required.";
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
    }
}
