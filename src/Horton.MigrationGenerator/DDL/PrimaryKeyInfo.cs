using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Horton.MigrationGenerator.DDL
{
    public class PrimaryKeyInfo : ITableConstraintInfo
    {
        public string PrimaryKeyName { get; set; }
        public bool IsNonClustered { get; set; }
        public IEnumerable<string> Columns { get; internal set; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.Write($"CONSTRAINT [{PrimaryKeyName}] PRIMARY KEY {(IsNonClustered ? "NONCLUSTERED" : "CLUSTERED")} ({string.Join(",", Columns)})");
        }
    }
}
