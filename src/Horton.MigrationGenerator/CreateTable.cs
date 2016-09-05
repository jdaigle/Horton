using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horton.MigrationGenerator
{
    public class CreateTable : AbstractDatabaseChange
    {
        public CreateTable(string table, string schema)
        {
            SchemaName = schema;
            TableName = table;
            if (string.IsNullOrEmpty(SchemaName))
            {
                SchemaName = "dbo";
            }
            Columns = new List<ColumnInfo>();
        }

        public string SchemaName { get; }
        public string TableName { get; }

        public List<ColumnInfo> Columns { get; }

        public void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{SchemaName}].[{TableName}]'))");
            textWriter.WriteLine($"CREATE TABLE [{SchemaName}].[{TableName}] (");
            textWriter.Indent++;

            foreach (var column in Columns)
            {
                column.AppendDDL(textWriter);
                textWriter.WriteLine(",");
            }

            textWriter.Indent--;
            textWriter.WriteLine(");");
            textWriter.WriteLine("GO");
        }
    }
}
