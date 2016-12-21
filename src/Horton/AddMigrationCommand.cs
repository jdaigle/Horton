using System;
using System.IO;
using System.Linq;

namespace Horton
{
    public sealed class AddMigrationCommand : HortonCommand
    {
        public override string Name { get { return "ADD-MIGRATION"; } }
        public override string Description { get { return "Scaffolds a blank migration. Optionally specify a name for the migration as the last argument."; } }

        public override void Execute(HortonOptions options)
        {
            if (!options.Unattend)
            {
                Program.PrintLine("=== Add Migration ===");
                Program.PrintLine();
            }

            var migrationName = "";
            if (options.ExtraArguments.Length > 0)
            {
                migrationName = options.ExtraArguments[0];
            }
            while (string.IsNullOrEmpty(migrationName))
            {
                if (options.Unattend)
                {
                    Program.PrintErrorLine("Migration name is required while running in Unattend mode. Specificy Migration name as the last argument.");
                    Environment.Exit(1);
                }
                Program.PrintLine("Please enter a name for the migration and press return.");
                migrationName = Console.ReadLine();
                Program.PrintLine();
            }

            migrationName = migrationName.Replace(" ", "_");

            var loader = new FileLoader(options.MigrationsDirectoryPath);
            loader.LoadAllFiles();
            var lastMigrationScript = loader.Files.OfType<MigrationScript>().OrderBy(x => x.SerialNumber).Last();

            var newSerialNumber = lastMigrationScript.SerialNumber + 1;
            var serialNumberDigits = lastMigrationScript.FileName.IndexOf('_');
            var newMigrationDirectory = Path.GetDirectoryName(lastMigrationScript.FilePath);
            var migrationFileName = newSerialNumber.ToString().PadLeft(serialNumberDigits, '0') + "_" + migrationName + ".sql";
            var newMigrationFullFilePath = Path.Combine(newMigrationDirectory, migrationFileName);

            Program.Print("Writing migration: ");
            Program.PrintLine(ConsoleColor.DarkGreen, newMigrationFullFilePath);

            File.WriteAllText(newMigrationFullFilePath, "");

            if (!options.Unattend)
            {
                System.Diagnostics.Process.Start(newMigrationFullFilePath);
                Program.PrintLine();
                Program.PrintLine("Finished.");
            }
        }
    }
}
