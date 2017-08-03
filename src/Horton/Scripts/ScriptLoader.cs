using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Horton.Scripts
{
    public class ScriptLoader
    {
        private readonly string path;

        public ScriptLoader(string path)
        {
            this.path = path;
        }

        public IReadOnlyList<ScriptFile> Files { get; private set; }

        public void LoadAllFiles()
        {
            var files = new List<ScriptFile>();
            Files = files;
            files.AddRange(LoadScripts(Path.Combine(path, "/migrations"), MigrationScript.Load));
            files.AddRange(LoadScripts(Path.Combine(path, "/objects"), ObjectScript.Load));
        }

        private static List<ScriptFile> LoadScripts(string path, Func<FileInfo, ScriptFile> load)
        {
            var dir = new DirectoryInfo(path);
            var scripts = dir.GetFiles("*.sql", SearchOption.AllDirectories)
                           .Select(x => load(x))
                           .Where(x => x != null)
                           .ToList();

            var duplicates = scripts.GroupBy(x => x.FileName).Where(x => x.Count() > 1);
            if (duplicates.Any())
            {
                var sb = new StringBuilder();
                foreach (var duplicate in duplicates)
                {
                    sb.AppendLine($"Duplicate Filename Detected: \"{duplicate.First().FileName}\"");
                    foreach (var file in duplicate)
                    {
                        sb.AppendLine($"\t\"{file.FilePath}\"");
                    }
                }
                sb.AppendLine();
                sb.AppendLine("All filenames must be unique. Including those in separate subdirectories.");
                throw new DuplicateFilenameException(sb.ToString());
            }

            return scripts;
        }
    }
}
