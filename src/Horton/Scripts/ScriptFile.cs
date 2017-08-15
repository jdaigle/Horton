using System;
using System.IO;
using System.Text;

namespace Horton.Scripts
{
    public abstract class ScriptFile : IComparable, IComparable<ScriptFile>
    {
        public static ScriptFile Load(FileInfo x)
        {
            var underscoreIndex = x.Name.IndexOf("_");
            if (underscoreIndex > 0)
            {
                var prefix = x.Name.Substring(0, underscoreIndex);
                if (int.TryParse(prefix, out int serialNumber))
                {
                    return new MigrationScript(x.FullName, x.Name, serialNumber);
                }
            }
            return new ObjectScript(x.FullName, x.Name);
        }

        protected ScriptFile(string filePath, string fileName)
        {
            FilePath = filePath;

            FileName = fileName;

            Content = File.ReadAllText(FilePath, Encoding.UTF8);
            ContentHash = Content.SHA1Hash();
        }

        public string FilePath { get; }

        public string FileName { get; }

        public string Content { get; }
        public string ContentHash { get; }

        public abstract byte TypeCode { get; }
        public abstract bool ConflictOnContent { get; }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (obj is ScriptFile other)
            {
                return CompareTo(other);
            }

            throw new InvalidOperationException($"{nameof(obj)} is not {typeof(ScriptFile).FullName}");
        }

        public virtual int CompareTo(ScriptFile other) => FileName.CompareTo(other.FileName);
    }
}
