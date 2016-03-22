using System;
using System.Linq;
using Horton.SqlServer;

namespace Horton
{
    internal class SyncCommand : HortonCommand
    {
        public override void Execute(HortonOptions options)
        {
            using (var schemaInfo = new SchemaInfo(options))
            {
                schemaInfo.InitializeTable();

                var loader = new FileLoader(options.MigrationsDirectoryPath);
                loader.LoadAllFiles();

                Console.WriteLine("=== Sync ===");
                Console.WriteLine();
                Console.WriteLine("Synchronizing conflicting scripts...");

                foreach (var file in loader.Files)
                {
                    var existingRecord = schemaInfo.AppliedMigrations.SingleOrDefault(x => x.FileNameMD5Hash == file.FileNameHash);
                    if (existingRecord != null)
                    {
                        if (file.ContentConflict(existingRecord.ContentSHA1Hash))
                        {
                            var prevColor = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write($"\n\"{file.FileName}\" has changed since it was applied on \"{existingRecord.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")}\" and will be updated.");
                            Console.Write(" Type 'Y' to Continue.");
                            Console.WriteLine();
                            Console.ForegroundColor = prevColor;
                            var c = Console.ReadKey();
                            if (c.KeyChar == 'y' || c.KeyChar == 'Y')
                            {
                                Console.WriteLine($"\nUpdating \"{file.FileName}\" with hash \"{file.ContentSHA1Hash}\"");
                                schemaInfo.ResyncMigration(file);
                            }
                            else
                            {
                                Console.WriteLine("\nAborting...");
                                return;
                            }
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Finished.");
            }
        }
    }
}