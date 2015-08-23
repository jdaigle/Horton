using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SqlMigrate
{
    public class Configuration
    {
        private static Configuration world;

        public static Configuration World
        {
            get
            {
                if (world == null)
                    world = new Configuration();
                return world;
            }
        }

        private Configuration()
        {
        }

        public string Server { get; set; }
        public string Database { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Apply { get; set; }

        public string ScriptsDirectoryPath { get; set; }
        public bool Unattended { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(ScriptsDirectoryPath) &&
                       !string.IsNullOrEmpty(Server) &&
                       !string.IsNullOrEmpty(Database) &&
                       (IntegratedSecurity || (!string.IsNullOrEmpty(Username) && 
                                               !string.IsNullOrEmpty(Password)));
            }
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            output.AppendLine("World Configuration");
            output.AppendLine("---");
            output.AppendLine("Script Directory Path: [" + ScriptsDirectoryPath + "]");
            output.AppendLine("Server: [" + Server + "]");
            output.AppendLine("Database: [" + Database + "]");
            output.AppendLine("Integrated Security: [" + (IntegratedSecurity ? "Yes" : "No") + "]");
            if (!IntegratedSecurity)
            {
                output.AppendLine("Username: [" + Username + "]");
                output.AppendLine("Password: [" + Password + "]");
            }
            return output.ToString();
        }
    }
}
