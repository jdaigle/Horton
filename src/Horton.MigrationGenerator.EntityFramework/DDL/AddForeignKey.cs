using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public class AddForeignKey : AbstractDatabaseChange
    {
        public string ForeignKeyObjectIdentifier { get; set; }
        public string ParentObjectIdentifier { get; set; }
        public string ParentObjectColumnName { get; set; }
        public string ReferencedObjectIdentifier { get; set; }
        public string ReferencedObjectColumnName { get; set; }

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{ForeignKeyObjectIdentifier}'))");

            textWriter.Indent++;
            textWriter.WriteLine($"ALTER TABLE {ParentObjectIdentifier}");
            textWriter.Indent++;
            textWriter.WriteLine($"ADD CONSTRAINT {ForeignKeyObjectIdentifier} FOREIGN KEY ([{ParentObjectColumnName}])");
            textWriter.WriteLine($"    REFERENCES {ReferencedObjectIdentifier} ([{ReferencedObjectColumnName}]);");
            textWriter.Indent--;
            textWriter.Indent--;

            textWriter.WriteLine("GO");
        }
    }
}
