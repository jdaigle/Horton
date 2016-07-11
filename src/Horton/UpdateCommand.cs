using System;
using System.Collections.Generic;
using System.Linq;
using Horton.SqlServer;

namespace Horton
{
    internal class UpdateCommand : HortonCommand
    {
        public override void Execute(HortonOptions options)
        {
            using (var schemaInfo = new SchemaInfo(options))
            {
                schemaInfo.InitializeTable();

                var loader = new FileLoader(options.MigrationsDirectoryPath);
                loader.LoadAllFiles();

                Console.WriteLine("=== Info ===");
                Console.WriteLine();
                Console.WriteLine("The following scripts will execute...");

                var toExecute = new List<ScriptFile>();
                // XXX hack to run repeatable files without trying to overwrite schema info in DB
                var dontRecordThese = new List<ScriptFile>();
                bool willExecuteMigrations = true;

                foreach (var file in loader.Files)
                {
                    var existingRecord = schemaInfo.AppliedMigrations.SingleOrDefault(x => x.FileNameMD5Hash == file.FileNameHash);
                    if (existingRecord == null)
                    {
                        var prevColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"\n\"{file.FileName}\" will execute on UPDATE.");
                        Console.ForegroundColor = prevColor;
                        toExecute.Add(file);
                    }
                    else if (file.TypeCode == 2)
                    {
                        // typeCode 2 is a repeatable file, it will always run (procs/views have desired state)
                        var prevColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"\n\"{file.FileName}\" will always execute on UPDATE, since it's a repeatable file.");
                        Console.ForegroundColor = prevColor;
                        toExecute.Add(file);
                        // XXX hack to run repeatable files without trying to overwrite schema info in DB
                        dontRecordThese.Add(file);
                    }
                    else if (file.ContentConflict(existingRecord.ContentSHA1Hash))
                    {
                        var prevColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nCONFLICT: The script \"{file.FileName}\" has changed since it was applied on \"{existingRecord.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")}\".");
                        Console.ForegroundColor = prevColor;
                        willExecuteMigrations = false;
                    }
                }

                if (!willExecuteMigrations)
                {
                    var prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nWARNING! Migrations will not execute until conflicts are resolved.");
                    Console.ForegroundColor = prevColor;
                    return;
                }

                if (!options.Unattend && toExecute.Any())
                {
                    Console.WriteLine($"\nAbout to execute {toExecute.Count} scripts. Press 'y' to continue.");
                    var c = Console.ReadKey();
                    Console.WriteLine();
                    if (c.KeyChar != 'y' && c.KeyChar != 'Y')
                    {
                        Console.WriteLine("Aborting...");
                        return;
                    }
                }

                foreach (var file in toExecute)
                {
                    var prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"\nApplying \"{file.FileName}\"... ");

                    // XXX hack to run repeatable files without trying to overwrite schema info in DB
                    bool shouldRecord = !dontRecordThese.Contains(file);
                    schemaInfo.ApplyMigration(file, shouldRecord);

                    Console.WriteLine("done.");
                    Console.ForegroundColor = prevColor;
                }

                Console.WriteLine();
                Console.WriteLine("Finished.");
            }
        }
    }
}