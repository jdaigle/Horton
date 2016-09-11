using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Horton.MigrationGenerator.DDL
{
    public class ForeignKeyInfo : ITableConstraintInfo
    {
        public string ForeignKeyObjectIdentifier { get; set; }
        public string ParentObjectIdentifier { get; set; }
        public IEnumerable<string> ParentObjectColumns { get; set; }
        public string ReferencedObjectIdentifier { get; set; }
        public IEnumerable<string> ReferencedObjectColumns { get; set; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"CONSTRAINT [{ForeignKeyObjectIdentifier}]");
            textWriter.WriteLine($"    FOREIGN KEY ({string.Join(",", ParentObjectColumns.Select(c => "[" + c + "]"))})");
            textWriter.Write($"    REFERENCES {ReferencedObjectIdentifier} ({string.Join(",", ReferencedObjectColumns.Select(c => "[" + c + "]"))})");
        }
    }
}
