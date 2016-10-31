namespace Horton
{
    public class RepeatableScript : ScriptFile
    {
        public RepeatableScript(string filePath, string fileName)
            :base (filePath, fileName)
        {
        }

        public override byte TypeCode => 2;
        public override bool ConflictOnContent => false;
    }
}