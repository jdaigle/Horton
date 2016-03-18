using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        }
    }
}
