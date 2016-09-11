using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public class AlterColumn : AbstractDatabaseChange
    {
        public AlterColumn(string objectIdentifier, ColumnInfo column, string note)
        {
            ObjectIdentitifer = objectIdentifier;
            Column = column;
            Note = note ?? "";
        }

        public string ObjectIdentitifer { get; }
        public ColumnInfo Column { get; }
        public string Note { get; }

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            if (Note.Length > 0)
            {
                textWriter.WriteLine("/*");
                textWriter.Write("  ");
                textWriter.WriteLine(Note);
                textWriter.WriteLine("*/");
            }

            textWriter.WriteLine($"IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'{ObjectIdentitifer}') AND name = '{Column.Name}')");

            textWriter.Indent++;
            textWriter.WriteLine($"ALTER TABLE {ObjectIdentitifer}");

            textWriter.Indent++;
            textWriter.Write($"ALTER COLUMN ");
            Column.AppendDDL(textWriter, includeConstraints: false);
            textWriter.WriteLine(";");
            textWriter.Indent--;

            textWriter.Indent--;

            textWriter.WriteLine("GO");
        }
    }
}
