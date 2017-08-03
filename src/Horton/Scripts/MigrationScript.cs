using System;
using System.IO;

namespace Horton.Scripts
{
    public class MigrationScript : ScriptFile, IComparable<MigrationScript>
    {
        public static MigrationScript Load(FileInfo x)
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
            return null;
        }

        public MigrationScript(string filePath, string fileName, int serialNumber)
            : base(filePath, fileName)
        {
            SerialNumber = serialNumber;
        }

        public int SerialNumber { get; }

        public override byte TypeCode => 1;

        public override bool ConflictOnContent => true;

        public override int CompareTo(ScriptFile other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is MigrationScript otherMigrationScript)
            {
                return CompareTo(otherMigrationScript);
            }

            if (other is ObjectScript)
            {
                return -1;
            }

            return -1;
        }

        public int CompareTo(MigrationScript other) => SerialNumber.CompareTo(other.SerialNumber);
    }
}