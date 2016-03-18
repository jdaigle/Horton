using System;

namespace Horton
{
    public class HortonOptions
    {
        public string Command { get; set; } = "";

        public string MigrationsDirectoryPath { get; set; } = Environment.CurrentDirectory;
        public string ServerHostname { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "";

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public bool Unattend { get; set; } = false;

        public bool IsIntegratedSecurity => string.IsNullOrWhiteSpace(Username);

        public bool AssertValid(out string firstValidationMessage)
        {
            if (string.IsNullOrWhiteSpace(Command))
            {
                firstValidationMessage = "Command is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                firstValidationMessage = "Database Name is required.";
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
