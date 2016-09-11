using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public class AddForeignKey : AbstractDatabaseChange
    {
        public AddForeignKey(ForeignKeyInfo foreignKey)
        {
            ForeignKey = foreignKey;
        }

        public ForeignKeyInfo ForeignKey { get; }

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{ForeignKey.ForeignKeyObjectIdentifier}'))");

            textWriter.Indent++;
            textWriter.WriteLine($"ALTER TABLE {ForeignKey.ParentObjectIdentifier}");
            textWriter.Indent++;
            textWriter.Write("ADD");
            textWriter.Indent++;
            ForeignKey.AppendDDL(textWriter);
            textWriter.WriteLine(";");
            textWriter.Indent--;
            textWriter.Indent--;
            textWriter.Indent--;

            textWriter.WriteLine("GO");
        }
    }
}
