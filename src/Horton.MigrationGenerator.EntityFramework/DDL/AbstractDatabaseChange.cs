using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public abstract class AbstractDatabaseChange
    {
        public abstract void AppendDDL(IndentedTextWriter textWriter);
    }
}
