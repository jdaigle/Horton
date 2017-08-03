namespace Horton.Scripts
{
    public class ObjectScript : ScriptFile
    {
        public ObjectScript(string filePath, string fileName)
            : base(filePath, fileName)
        {
        }

        public override byte TypeCode => 2;

        public override bool ConflictOnContent => false;
    }
}