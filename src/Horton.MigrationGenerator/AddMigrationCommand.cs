using System;
using System.CodeDom.Compiler;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Horton.MigrationGenerator.EF6;

namespace Horton.MigrationGenerator
{
    public sealed class AddMigrationCommand : HortonCommand
    {
        public override string Name { get { return "ADD-MIGRATION"; } }
        public override string Description { get { return "Scaffolds a new migration based on the EF6 entity model compared to the phsyical database."; } }

        public override void Execute(HortonOptions options)
        {
            Program.PrintLine("=== Add Migration ===");
            Program.PrintLine();

            Program.Print("Loading DbContext...");
            var dbContext = LoadDbContext(options);
            Program.PrintLine("...Done.");
            Program.PrintLine();

            Program.Print("Comparing EF Model to Physical Database...");
            var changes = new DiffTool(((IObjectContextAdapter)dbContext).ObjectContext, new SqlConnection(options.CreateConnectionString())).FindChanges();
            Program.PrintLine("...Done.");
            Program.PrintLine();

            Program.Print(ConsoleColor.DarkGreen, "Detected ");
            Program.Print(ConsoleColor.DarkGreen, changes.Count.ToString());
            Program.PrintLine(ConsoleColor.DarkGreen, " Changes.");
            Program.PrintLine();


            var sb = new StringBuilder();
            var textWriter = new IndentedTextWriter(new StringWriter(sb));
            foreach (var change in changes)
            {
                change.AppendDDL(textWriter);
                textWriter.WriteLine();
                textWriter.WriteLine();
            }
            if (sb.Length > 0)
            {
                var migrationName = "";
                while (string.IsNullOrEmpty(migrationName))
                {
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
                Program.PrintLine(ConsoleColor.DarkGreen, migrationFileName);

                File.WriteAllText(newMigrationFullFilePath, sb.ToString());

                System.Diagnostics.Process.Start(newMigrationFullFilePath);
            }

            Program.PrintLine();
            Program.PrintLine("Finished.");
        }

        private DbContext LoadDbContext(HortonOptions options)
        {
            var dbContextAssemblyPath = TryGetDbContextAssemblyPathFromFile(options.MigrationsDirectoryPath);
            if (string.IsNullOrEmpty(dbContextAssemblyPath))
            {
                throw new Exception("Error Loading EF DbContext Assembly: Cannot determine path, please create a \"dbcontext.path\" file in migration directory.");
            }

            if (!File.Exists(dbContextAssemblyPath))
            {
                throw new Exception($"Error Loading EF DbContext Assembly: File not found [{dbContextAssemblyPath}].");
            }

            var asm = Assembly.LoadFrom(dbContextAssemblyPath);

            Type dbContextType = null;
            foreach (var type in asm.GetLoadableTypes())
            {
                if (typeof(DbContext).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    dbContextType = type;
                    break;
                }
            }

            if (dbContextType == null)
            {
                throw new Exception($"Error Loading EF DbContext Assembly: Cannot find concrete DbContext type in [{dbContextAssemblyPath}]");
            }

            var connectionString = options.CreateConnectionString();
            Program.PrintLine();
            Program.Print("Found: ");
            Program.Print(dbContextType.FullName);
            Program.PrintLine();
            return Activator.CreateInstance(dbContextType, new[] { connectionString }) as DbContext;
        }


        private static string TryGetDbContextAssemblyPathFromFile(string migrationDirectoryPath)
        {
            var dbContextPathFile = Path.Combine(migrationDirectoryPath, "dbcontext.path");
            if (File.Exists(dbContextPathFile))
            {
                var lines = File.ReadAllLines(dbContextPathFile);
                if (lines.Length > 0)
                {
                    var path = lines[0].Trim();
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    if (path.StartsWith("."))
                    {
                        path = Path.Combine(migrationDirectoryPath, path);
                    }
                    return path;
                }
            }
            return "";
        }
    }
}
