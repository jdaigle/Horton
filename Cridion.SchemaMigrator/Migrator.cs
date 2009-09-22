using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace Cridion.SchemaMigrator
{
    public class Migrator
    {

        public Migrator(String ConnectionString, String MigrationDirectory, Boolean SQL, Boolean SMO)
        {
            Configuration.CurrentConfig = new Configuration { ConnectionString = ConnectionString, MigrationDirectory = MigrationDirectory, UsesSQL = SQL, UsesSMO = SMO };
        }

        public Migrator(Configuration Configuration)
        {
            Configuration.CurrentConfig = Configuration;
        }

        public void Migrate(int TargetVersion)
        {

            /* get and sort the files */
            FileInfo[] files = GetMigrationFiles();

            /* get the latest version */
            int LatestVersion = Int32.Parse(files[files.Length - 1].Name.Substring(0, files[files.Length - 1].Name.IndexOf("_")));
            int CurrentVersion = SchemaManager.GetVersionNumber();
            if (TargetVersion < 0)
                TargetVersion = LatestVersion;

            Console.WriteLine("Version At: " + CurrentVersion);

            if (TargetVersion > CurrentVersion)
                GoUp(CurrentVersion, TargetVersion, files);
            else if (TargetVersion < CurrentVersion)
                GoDown(CurrentVersion, TargetVersion, files);
        }

        /* get and sort the files */
        public FileInfo[] GetMigrationFiles()
        {
            DirectoryInfo migrationsDirectory = new DirectoryInfo(Configuration.CurrentConfig.MigrationDirectory);

            FileInfo[] files = null;
            if (Configuration.CurrentConfig.UsesSMO)
            {
                files = migrationsDirectory.GetFiles("*.cs", SearchOption.TopDirectoryOnly);
            }
            if (Configuration.CurrentConfig.UsesSQL)
            {
                files = migrationsDirectory.GetFiles("*.sql", SearchOption.TopDirectoryOnly);
            }

            FileNameComparer[] compareFiles = new FileNameComparer[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                compareFiles[i] = new FileNameComparer(files[i]);
            }
            Array.Sort(compareFiles);
            files = new FileInfo[compareFiles.Length];
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = compareFiles[i].FileInfo;
            }

            return files;
        }

        private Compiler compiler = null;

        public Migration GetMigration(String FullFileName, String Name)
        {
            if (Configuration.CurrentConfig.UsesSMO)
            {
                if (compiler == null)
                    compiler = new Compiler();
                return compiler.Compile(FullFileName);
            }
            if (Configuration.CurrentConfig.UsesSQL)
            {
                return new SQLMigration(FullFileName, Name);
            }
            throw new Exception("Unsupported Migration: Not SMO, Not SQL");
        }

        public FileInfo[] GetMatchingFiles(FileInfo[] files, String ToMatch)
        {
            ArrayList filesMatching = new ArrayList();
            foreach (FileInfo file in files)
            {
                if (file.Name.Contains(ToMatch))
                    filesMatching.Add(file);
            }
            FileInfo[] toReturn = new FileInfo[filesMatching.Count];
            for (int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = (FileInfo)filesMatching[i];
            }
            return toReturn;
        }

        public void GoUp(int CurrentVersion, int TargetVersion, FileInfo[] files)
        {
            if (Configuration.CurrentConfig.UsesSQL)
                files = GetMatchingFiles(files, "_up_");

            while (CurrentVersion != TargetVersion)
            {
                int NextVersion = CurrentVersion + 1;

                int VersionIndex = GetIndexOfVersion(files, NextVersion);

                Migration migration = GetMigration(files[VersionIndex].FullName, files[VersionIndex].Name);
                Console.WriteLine("Running Migration: " + migration.ToString() + "|" + files[VersionIndex].Name);
                migration.Up();
                Console.WriteLine("Ran Migration: " + migration.ToString() + "|" + files[VersionIndex].Name);

                SchemaManager.SetVersionNumber(NextVersion);

                CurrentVersion = SchemaManager.GetVersionNumber();
                Console.WriteLine("Version At: " + CurrentVersion);
            }
        }

        public void GoDown(int CurrentVersion, int TargetVersion, FileInfo[] files)
        {
            if (Configuration.CurrentConfig.UsesSQL)
                files = GetMatchingFiles(files, "_down_");

            while (CurrentVersion != TargetVersion)
            {
                int PreviousVersion = CurrentVersion - 1;
                int VersionIndex = GetIndexOfVersion(files, CurrentVersion);

                Migration migration = GetMigration(files[VersionIndex].FullName, files[VersionIndex].Name);
                Console.WriteLine("Rolling Back Migration: " + migration.ToString() + "|" + files[VersionIndex].Name);
                migration.Down();
                Console.WriteLine("Rolled Back Migration: " + migration.ToString() + "|" + files[VersionIndex].Name);

                SchemaManager.SetVersionNumber(PreviousVersion);

                CurrentVersion = SchemaManager.GetVersionNumber();
                Console.WriteLine("Version At: " + CurrentVersion);
            }
        }



        public int GetIndexOfVersion(FileInfo[] files, int Version)
        {
            for (int i = 0; i < files.Length; i++)
            {
                int thisversion = Int32.Parse(files[i].Name.Substring(0, files[i].Name.IndexOf("_")));
                if (thisversion == Version)
                    return i;
            }

            throw new Exception("Version File Not found!!!");
        }
    }
}
