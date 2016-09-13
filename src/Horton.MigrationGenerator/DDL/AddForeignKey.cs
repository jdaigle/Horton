using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public class AddForeignKey : AbstractDatabaseChange
    {
        public AddForeignKey(ForeignKeyInfo foreignKey, string note)
        {
            ForeignKey = foreignKey;
            Note = note;
        }

        public ForeignKeyInfo ForeignKey { get; }
        public string Note { get;}

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            if (!string.IsNullOrEmpty(Note))
            {
                textWriter.WriteLine("/*");
                textWriter.Write("  ");
                textWriter.WriteLine(Note);
                textWriter.WriteLine("*/");
            }

            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{ForeignKey.QuotedForeignKeyName}'))");

            textWriter.Indent++;
            textWriter.WriteLine($"ALTER TABLE {ForeignKey.ParentObjectIdentifier}");
            textWriter.Indent++;
            textWriter.Write("ADD ");
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
