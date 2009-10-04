using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace SqlPatch
{
    public class Program
    {

        public static void Main(string[] args)
        {
            if (!ParseCommandLineArgs(args))
            {
                OutputHelpMessage();
                return;
            }
            Console.WriteLine("\nStarting Migration\n");
            new MigrationEngine().Migrate();
            Console.WriteLine("\nMigration Complete\n");
        }

        private static void OutputHelpMessage()
        {
            var output = new StringBuilder();
            output.AppendLine();
            output.AppendLine("Simple Sql Patcher Command Line Arguments");
            output.AppendLine("---");
            output.AppendLine("/m PATH\t\tMigration Directory Path (no spaces or surrounded by quotes)");
            output.AppendLine("/s SERVER\tSQL Server Network Address");
            output.AppendLine("/d DATABASE\tSQL Server Database Name");
            output.AppendLine("/i \t\tIntegrated SQL Server Security");
            output.AppendLine("/u USERNAME\tSQL Server Login Username");
            output.AppendLine("/p PASSWORD\tSQL Server Login Password");
            output.AppendLine();
            output.AppendLine("EXAMPLES:");
            output.AppendLine(@"SqlPatch.exe /m \Migrations /s .\SQLEXPRESS /d Northwind /i");
            output.AppendLine(@"SqlPatch.exe /m \Migrations /s .\SQLEXPRESS /d Northwind /u sa /p pa55w0rd");
            output.AppendLine("SqlPatch.exe /m \"c:\\Example Folder\\Migrations\" /s .\\SQLEXPRESS /d Northwind /i");
            output.AppendLine();
            Console.Out.WriteLine(output.ToString());
        }

        private static bool ParseCommandLineArgs(string[] args)
        {
            if (args.Length < 7)
                return false;
            for (int i = 0; i < args.Length; i++)
            {
                bool lastArg = i == args.Length - 1;
                var argument = args[i];
                if (argument.Equals("/m", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.MigrationDirectoryPath = args[i + 1];
                }
                else if (argument.Equals("/s", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Server = args[i + 1];
                }
                else if (argument.Equals("/d", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Database = args[i + 1];
                }
                else if (argument.Equals("/i", StringComparison.OrdinalIgnoreCase))
                {
                    Configuration.World.IntegratedSecurity = true;
                }
                else if (argument.Equals("/u", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Username = args[i + 1];
                }
                else if (argument.Equals("/p", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Password = args[i + 1];
                }
            }
            return Configuration.World.IsValid;
        }

    }
}
