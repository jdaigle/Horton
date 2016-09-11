using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public class TableCheckConstraintInfo : ITableConstraintInfo
    {
        public string ConstraintName { get; set; }
        public bool IsSystemNamed { get; internal set; }
        public string Definition { get; set; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            if (!IsSystemNamed)
            {
                textWriter.Write($"CONSTRAINT {ConstraintName} ");
            }
            textWriter.Write("CHECK ");
            textWriter.Write(Definition);
        }
    }
}
