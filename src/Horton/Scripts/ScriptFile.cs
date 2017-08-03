using System;
using System.IO;
using System.Text;

namespace Horton.Scripts
{
    public abstract class ScriptFile : IComparable, IComparable<ScriptFile>
    {
        protected ScriptFile(string filePath, string fileName)
        {
            FilePath = filePath;

            FileName = fileName;
            FileNameHash = FileName.SHA1Hash();

            Content = File.ReadAllText(FilePath, Encoding.UTF8);
            ContentHash = Content.SHA1Hash();
        }

        public string FilePath { get; }

        public string FileName { get; }
        public byte[] FileNameHash { get; }

        public string Content { get; }
        public byte[] ContentHash { get; }

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
