using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horton.MigrationGenerator.DDL
{
    public class AddColumn : AbstractDatabaseChange
    {
        public AddColumn(string objectIdentifier, ColumnInfo column)
        {
            ObjectIdentitifer = objectIdentifier;
            Column = column;
        }

        public string ObjectIdentitifer { get; }
        public ColumnInfo Column { get; }

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'{ObjectIdentitifer}') AND name = '{Column.Name}')");

            textWriter.Indent++;
            textWriter.WriteLine($"ALTER TABLE {ObjectIdentitifer}");

            textWriter.Indent++;
            textWriter.Write($"ADD COLUMN ");
            Column.AppendDDL(textWriter, includeConstraints: true);
            textWriter.WriteLine(";");
            textWriter.Indent--;

            textWriter.Indent--;

            textWriter.WriteLine("GO");
        }
    }
}
