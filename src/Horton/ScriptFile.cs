using System;
using System.IO;
using System.Text;

namespace Horton
{
    public class ScriptFile : IComparable, IComparable<ScriptFile>
    {
        public static ScriptFile Load(FileInfo x)
        {
            int serialNumber = 0;
            bool isMigration = false;
            var underscoreIndex = x.Name.IndexOf("_");
            if (underscoreIndex > 0)
            {
                var prefix = x.Name.Substring(0, underscoreIndex);
                isMigration = int.TryParse(prefix, out serialNumber);
            }
            if (isMigration)
            {
                return new MigrationScript(x.FullName, x.Name, serialNumber);
            }
            else
            {
                return new RepeatableScript(x.FullName, x.Name);
            }
        }

        protected ScriptFile(string filePath, string fileName)
        {
            FilePath = filePath;

            FileName = fileName;
            FileNameHash = FileName.MD5Hash();

            Content = File.ReadAllText(FilePath, Encoding.UTF8);
            ContentSHA1Hash = Content.SHA1Hash();
        }

        public string FilePath { get; }

        public string FileName { get; }
        public Guid FileNameHash { get; }

        public string Content { get; }
        public string ContentSHA1Hash { get; }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
                return 0;

            var other = obj as ScriptFile;
            if (other != null)
                return CompareTo(other);

            throw new InvalidOperationException($"{nameof(obj)} is not {typeof(ScriptFile).FullName}");
        }

        public virtual int CompareTo(ScriptFile other)
        {
            return FileName.CompareTo(other.FileName);
        }
    }
}
