using System;
using System.Linq;
using Horton.SqlServer;

namespace Horton
{
    internal class InfoCommand : HortonCommand
    {
        public override void Execute(HortonOptions options)
        {
            var schemaInfo = new SchemaInfo(options);
            schemaInfo.InitializeTable();

            var loader = new FileLoader(options.MigrationsDirectoryPath);
            loader.LoadAllFiles();

            Console.WriteLine("=== Info ===");
            Console.WriteLine();
            Console.WriteLine("The following scripts will execute...");

            bool willExecuteMigrations = true;

            foreach (var file in loader.Files)
            {
                var existingRecord = schemaInfo.AppliedMigrations.SingleOrDefault(x => x.FileNameMD5Hash == file.FileNameHash);
                if (existingRecord != null)
                {
                    if (file.ContentConflict(existingRecord.ContentSHA1Hash))
                    {
                        var prevColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nCONFLICT: The script \"{file.FileName}\" has changed since it was applied on \"{existingRecord.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")}\".");
                        Console.ForegroundColor = prevColor;
                        willExecuteMigrations = false;
                    }
                }
                else
                {
                    var prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\n\"{file.FileName}\" will execute on UPDATE.");
                    Console.ForegroundColor = prevColor;
                }
            }

            if (!willExecuteMigrations)
            {
                var prevColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nWARNING! Migrations will not execute until conflicts are resolved.");
                Console.ForegroundColor = prevColor;
            }

            Console.WriteLine();
            Console.WriteLine("Finished.");
        }
    }
}