using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Horton.MigrationGenerator.DDL
{
    public class UniqueConstraintInfo : ITableConstraintInfo
    {
        public string ConstraintName { get; set; }
        public bool IsSystemNamed { get; internal set; }
        public bool IsNonClustered { get; set; }
        public IEnumerable<string> Columns { get; internal set; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            if (!IsSystemNamed)
            {
                textWriter.Write($"CONSTRAINT [{ConstraintName}] ");
            }
            textWriter.Write($"UNIQUE {(IsNonClustered ? "NONCLUSTERED" : "CLUSTERED")} ({string.Join(",", Columns)})");
        }
    }
}
