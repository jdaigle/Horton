using System;
using System.Collections.Generic;
using System.Linq;
using Horton.SqlServer;

namespace Horton
{
    internal class UpdateCommand : HortonCommand
    {
        public override string Name { get { return "UPDATE"; } }
        public override string Description { get { return "Executes current migrations if no conflicts exist."; } }

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

                var toExecute = new List<ScriptFile>();
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
                    toExecute.Add(file);
                }

                if (!willExecuteMigrations)
                {
                    Program.PrintLine(ConsoleColor.Red, $"\nWARNING! Migrations will not execute until conflicts are resolved.");
                    return;
                }

                if (!options.Unattend && toExecute.Any())
                {
                    Program.PrintLine($"\nAbout to execute {toExecute.Count} scripts. Press 'y' to continue.");
                    var c = Console.ReadKey();
                    Program.PrintLine();
                    if (c.KeyChar != 'y' && c.KeyChar != 'Y')
                    {
                        Program.PrintLine("Aborting...");
                        return;
                    }
                }

                foreach (var file in toExecute)
                {
                    Program.Print(ConsoleColor.DarkGreen, $"\nApplying \"{file.FileName}\"... ");
                    schemaInfo.ApplyMigration(file);
                    Program.PrintLine(ConsoleColor.DarkGreen, "done.");
                }

                Program.PrintLine();
                Program.PrintLine("Finished.");
            }
        }
    }
}