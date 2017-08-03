using System.IO;

namespace Horton.Scripts
{
    public class ObjectScript : ScriptFile
    {
        public static ObjectScript Load(FileInfo x)
        {
            return new ObjectScript(x.FullName, x.Name);
        }

        public ObjectScript(string filePath, string fileName)
            : base(filePath, fileName)
        {
        }

        public override byte TypeCode => 2;

        public override bool ConflictOnContent => false;
    }
}