using System;
using System.IO;

namespace Horton
{
    public class HortonOptions
    {
        public HortonCommand Command { get; set; }

        public string MigrationsDirectoryPath { get; set; } = Environment.CurrentDirectory;
        public string ServerHostname { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "";

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public bool Unattend { get; set; } = false;

        public bool IsIntegratedSecurity => string.IsNullOrWhiteSpace(Username);

        public bool AssertValid(out string firstValidationMessage)
        {
            if (Command == null)
            {
                firstValidationMessage = "Command is required.";
                return false;
            }

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

        public void TryGetDatabaseNameFromFile()
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
