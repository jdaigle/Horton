using System;
using System.Linq;
using Horton.SqlServer;

namespace Horton
{
    internal class SyncCommand : HortonCommand
    {
        public override string Name { get { return "SYNC"; } }
        public override string Description { get { return "Resolves migration conflicts by updated checksums in database schema_info table."; } }

        public override void Execute(HortonOptions options)
        {
            using (var schemaInfo = new SchemaInfo(options))
            {
                schemaInfo.InitializeTable();

                var loader = new FileLoader(options.MigrationsDirectoryPath);
                loader.LoadAllFiles();

                Program.PrintLine("=== Sync ===");
                Program.PrintLine();
                Program.PrintLine("Synchronizing conflicting scripts...");

                foreach (var file in loader.Files)
                {
                    var existingRecord = schemaInfo.AppliedMigrations.SingleOrDefault(x => x.FileNameMD5Hash == file.FileNameHash);
                    if (existingRecord != null)
                    {
                        if (!file.ContentMatches(existingRecord.ContentSHA1Hash))
                        {
                            Program.Print(ConsoleColor.Red, $"\n\"{file.FileName}\" has changed since it was applied on \"{existingRecord.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")}\" and will be updated.");
                            Program.Print(ConsoleColor.Red, " Type 'Y' to Continue.");
                            Program.PrintLine();
                            var c = Console.ReadKey();
                            if (c.KeyChar == 'y' || c.KeyChar == 'Y')
                            {
                                Program.PrintLine($"\nUpdating \"{file.FileName}\" with hash \"{file.ContentSHA1Hash}\"");
                                schemaInfo.ResyncMigration(file);
                            }
                            else
                            {
                                Program.PrintLine("\nAborting...");
                                Environment.Exit(1);
                            }
                        }
                    }
                }

                Program.PrintLine();
                Program.PrintLine("Finished.");
            }
        }
    }
}