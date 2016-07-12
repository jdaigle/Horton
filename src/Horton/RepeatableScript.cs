namespace Horton
{
    public class RepeatableScript : ScriptFile
    {
        public RepeatableScript(string filePath, string fileName)
            :base (filePath, fileName)
        {
        }

        public override byte TypeCode { get { return 2; } }
        public override bool ConflictOnContent { get { return false; } }
    }
}