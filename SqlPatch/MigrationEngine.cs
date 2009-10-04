using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SqlPatch
{
    public class MigrationEngine
    {
        public void Migrate()
        {
            SchemaHelpers.EnsureSchemaInfoTable();
            var versions = SchemaHelpers.GetVerionNumbers();
            var currentVersion = versions.Count > 0 ? versions.Max() : 0;
            FileInfo[] files = GetSortedMigrationFiles();

            foreach (var file in files)
            {
                var fileName = file.Name;
                var version = Int32.Parse(fileName.Substring(0, fileName.IndexOf('_')));
                if (currentVersion >= version)
                    continue;
                Console.WriteLine("Applying Migration: " + fileName);
                var migration = new SqlMigration(file.FullName, file.Name);
                migration.Execute();
                SchemaHelpers.InsertVersion(version, fileName);
                Console.WriteLine("Applied, schema is at version: " + version.ToString());
            }
        }

        private FileInfo[] GetSortedMigrationFiles()
        {
            var migrationsDirectory = new DirectoryInfo(Configuration.World.MigrationDirectoryPath);
            return migrationsDirectory.GetFiles("*.sql", SearchOption.TopDirectoryOnly).OrderBy(x =>
                {
                    return Int32.Parse(x.Name.Substring(0, x.Name.IndexOf('_')));
                }).ToArray();
        }
    }
}
