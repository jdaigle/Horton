using System;
using System.Collections.Generic;
using System.Linq;
using Horton.Scripts;
using Horton.SqlServer;

namespace Horton
{
    public class MigrationRunner
    {
        public void Execute(HortonOptions options)
        {
            using (var database = new SqlServerDatabase(options))
            {
                database.Initialize();

                var loader = new ScriptLoader(options.FilesPath);
                loader.LoadAllFiles();

                var toExecute = new List<ScriptFile>();
                bool hasConflicts = false;

                Program.PrintLine("The following scripts will execute...");

                foreach (var file in loader.Files)
                {
                    var existingRecord = database.AppliedMigrations.FirstOrDefault(x => file.FileNameHash.HashMatches(x.FileNameHash));
                    if (existingRecord != null)
                    {
                        if (file.ContentHash.HashMatches(existingRecord.ContentHash))
                        {
                            continue;
                        }
                        if (file.ConflictOnContent)
                        {
                            Program.PrintErrorLine($"\nWARNING: The script \"{file.FileName}\" has changed since it was applied on \"{existingRecord.AppliedUTC:yyyy-MM-dd HH:mm:ss.ff}\".");
                            hasConflicts = true;
                            if (!options.WarnAndRerunModifiedMigrations)
                            {
                                continue;
                            }
                        }
                    }
                    Program.PrintSuccess($"\n\"{file.FileName}\" will execute.");
                    toExecute.Add(file);
                }

                if (hasConflicts && !options.WarnAndRerunModifiedMigrations)
                {
                    Program.PrintErrorLine($"\nWARNING: Migrations will not execute until conflicts are resolved.");
                    Environment.Exit(1);
                }

                if (!options.Unattend && toExecute.Any())
                {
                    Program.PrintLine($"\nAbout to execute {toExecute.Count} scripts. Press 'y' to continue.");
                    var c = Console.ReadKey();
                    if (c.KeyChar != 'y' && c.KeyChar != 'Y')
                    {
                        Program.PrintErrorLine("Aborting...");
                        Environment.Exit(1);
                    }
                }

                if (options.DryRun)
                {
                    Program.PrintLine("Dry Run Finished.");
                    return;
                }

                foreach (var file in toExecute)
                {
                    Program.PrintSuccess($"\nRunning \"{file.FileName}\"... ");
                    database.Run(file, options.RunBaseline);
                    Program.PrintSuccessLine("done.");
                }

                Program.PrintLine("Finished.");
            }
        }
    }
}