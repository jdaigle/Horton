using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Horton.MigrationGenerator.Sys;

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

        internal static CreateTable FromSQL(Table table)
        {
            var createTable = new CreateTable(SqlUtil.GetQuotedObjectIdentifierString(table.name, table.Schema.name), table.Columns.Select(c => ColumnInfo.FromSQL(c)) , "");

            return createTable;
        }
    }
}
