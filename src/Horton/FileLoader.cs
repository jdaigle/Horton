using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Horton
{
    public class FileLoader
    {
        private readonly string path;

        public FileLoader(string path)
        {
            this.path = path;
        }

        public IReadOnlyList<ScriptFile> Files { get; private set; }

        public void LoadAllFiles()
        {
            LoadChangeScripts();
        }

        private void LoadChangeScripts()
        {
            var scripts = new DirectoryInfo(path);
            Files = scripts.GetFiles("*.sql", SearchOption.AllDirectories)
                           .Select(x => ScriptFile.Load(x))
                           .OrderBy(x => x)
                           .ToList();

            var duplicates = Files.GroupBy(x => x.FileNameHash).Where(x => x.Count() > 1);
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
                throw new Exception(sb.ToString());
            }
        }
    }
}
