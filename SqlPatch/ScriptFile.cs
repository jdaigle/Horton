using System;
using System.IO;

namespace SqlPatch {
    public class ScriptFile : IScript {

        public ScriptFile(string filePath, string fileName, ScriptType type) {
            FilePath = filePath;
            FileName = fileName;
            Type = type;
        }

        public string FilePath { get; private set; }

        public Guid Id { get; private set; }
        public Guid ContentHash { get; private set; }
        public string FileName { get; private set; }
        public string Content { get; private set; }
        public ScriptType Type { get; private set; }

        public void Load() {
            Id = FileName.MD5Hash();
            using (var strm = File.OpenRead(FilePath)) {
                var reader = new StreamReader(strm);
                Content = reader.ReadToEnd();
            }
            ContentHash = Content.MD5Hash();
        }

        public bool Matches(IScript other) {
            return Id == other.Id &&
                ContentHash == other.ContentHash;
        }

        /// <summary>
        /// When true, we won't run the changes but we will track the file.
        /// </summary>
        public bool IgnoreChangesButTrackFile { get; set; }
    }
}
