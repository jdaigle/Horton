using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public interface ITableConstraintInfo
    {
        void AppendDDL(IndentedTextWriter textWriter);
    }
}
