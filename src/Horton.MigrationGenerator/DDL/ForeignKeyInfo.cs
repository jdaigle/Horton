using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public class ForeignKeyInfo : ITableConstraintInfo
    {
        public string ForeignKeyObjectIdentifier { get; set; }
        public string ParentObjectIdentifier { get; set; }
        public string ParentObjectColumnName { get; set; }
        public string ReferencedObjectIdentifier { get; set; }
        public string ReferencedObjectColumnName { get; set; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"CONSTRAINT {ForeignKeyObjectIdentifier}");
            textWriter.WriteLine($"    FOREIGN KEY ([{ParentObjectColumnName}])");
            textWriter.Write($"    REFERENCES {ReferencedObjectIdentifier} ([{ReferencedObjectColumnName}])");
        }
    }
}
