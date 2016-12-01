using System;
using System.Linq;
using Horton.SqlServer;

namespace Horton
{
    internal class InfoCommand : HortonCommand
    {
        public override string Name { get { return "INFO"; } }
        public override string Description { get { return "Prints the migrations that will execute on UPDATE. Prints any conflicting scripts."; } }

        public override void Execute(HortonOptions options)
        {
            using (var schemaInfo = new SchemaInfo(options))
            {
                schemaInfo.InitializeTable();

                var loader = new FileLoader(options.MigrationsDirectoryPath);
                loader.LoadAllFiles();

                Program.PrintLine("=== Info ===");
                Program.PrintLine();
                Program.PrintLine("The following scripts will execute...");

                bool willExecuteMigrations = true;

                foreach (var file in loader.Files)
                {
                    var existingRecord = schemaInfo.AppliedMigrations.SingleOrDefault(x => x.FileNameMD5Hash == file.FileNameHash);
                    if (existingRecord != null)
                    {
                        if (file.ContentMatches(existingRecord.ContentSHA1Hash))
                        {
                            continue;
                        }
                        if (file.ConflictOnContent)
                        {
                            Program.PrintLine(ConsoleColor.Red, $"\nCONFLICT: The script \"{file.FileName}\" has changed since it was applied on \"{existingRecord.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")}\".");
                            willExecuteMigrations = false;
                            continue;
                        }
                    }
                    Program.PrintLine(ConsoleColor.DarkGreen, $"\n\"{file.FileName}\" will execute on UPDATE.");
                }

                if (!willExecuteMigrations)
                {
                    Program.PrintLine(ConsoleColor.Red, $"\nWARNING! Migrations will not execute until conflicts are resolved.");
                    Environment.Exit(1);
                }

                Program.PrintLine();
                Program.PrintLine("Finished.");
            }
        }
    }
}