using System;

namespace Horton
{
    public class MigrationScript : ScriptFile, IComparable<MigrationScript>
    {
        public MigrationScript(string filePath, string fileName, int serialNumber)
            : base(filePath, fileName)
        {
            SerialNumber = serialNumber;
        }

        public int SerialNumber { get; }

        public override byte TypeCode { get { return 1; } }
        public override bool ConflictOnContent { get { return true; } }

        public override int CompareTo(ScriptFile other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            var otherMigrationScript = other as MigrationScript;
            if (otherMigrationScript != null)
                return CompareTo(otherMigrationScript);

            if (other is RepeatableScript)
                return -1;

            return -1;
        }

        public int CompareTo(MigrationScript other)
        {
            return SerialNumber.CompareTo(other.SerialNumber);
        }
    }
}