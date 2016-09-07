using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Horton.MigrationGenerator.DDL
{
    public class CreateTable : AbstractDatabaseChange
    {
        public CreateTable(string objectIdentifier, IEnumerable<ColumnInfo> columns, string note)
        {
            ObjectIdentifier = objectIdentifier;
            Columns = columns;
            Note = note ?? "";
        }

        public string ObjectIdentifier { get; }
        public IEnumerable<ColumnInfo> Columns { get; }
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

            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{ObjectIdentifier}'))");

            textWriter.Indent++;
            textWriter.WriteLine($"CREATE TABLE {ObjectIdentifier} (");

            textWriter.Indent++;
            foreach (var column in Columns)
            {
                column.AppendDDL(textWriter, includeDefaultConstraints: true);
                textWriter.WriteLine(",");
            }
            textWriter.Indent--;

            textWriter.WriteLine(");");
            textWriter.Indent--;

            textWriter.WriteLine("GO");
        }
    }
}
