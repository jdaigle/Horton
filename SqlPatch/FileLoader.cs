using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlPatch {
    public class FileLoader {

        private readonly string path;

        public FileLoader(string path) {
            this.path = path;
        }

        public Dictionary<Guid, ScriptFile> Files { get; private set; }

        public void LoadAllFiles() {
            var scripts = new DirectoryInfo(path);
            var files = scripts.GetFiles("*.sql", SearchOption.TopDirectoryOnly).OrderBy(x => {
                return Int32.Parse(x.Name.Substring(0, x.Name.IndexOf('_')));
            }).Select(x => new ScriptFile(x.FullName, x.Name)).ToList();
            Files = new Dictionary<Guid, ScriptFile>();
            foreach (var file in files) {
                file.Load();
                Files.Add(file.Id, file);
            }
        }
    }
}
