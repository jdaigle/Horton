using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Horton.MigrationGenerator.DDL
{
    public class CreateIndex : AbstractDatabaseChange
    {
        public string ObjectIdentitifer { get; set; }
        public string Name { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClusterd { get; set; }

        public string FilterDefinition { get; set; }
        public IEnumerable<string> KeyColumns { get; set; }
        public IEnumerable<string> IncludedColumns { get; set; }

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'{ObjectIdentitifer}') AND name = N'{Name}')");
            textWriter.WriteLine($"CREATE {(IsUnique ? "UNIQUE " : "")}{(IsClusterd ? "CLUSTERED" : "NONCLUSTERED")} INDEX [{Name}] ON {ObjectIdentitifer} (");
            textWriter.Write("    ");
            textWriter.WriteLine(string.Join("," + Environment.NewLine + "    ", KeyColumns));
            textWriter.Write(")");
            if (IncludedColumns != null && IncludedColumns.Any())
            {
                textWriter.WriteLine();
                textWriter.WriteLine("INCLUDE (");
                textWriter.Write("    ");
                textWriter.WriteLine(string.Join("," + Environment.NewLine + "    ", IncludedColumns));
                textWriter.Write(")");
            }
            if (!string.IsNullOrEmpty(FilterDefinition))
            {
                textWriter.WriteLine();
                textWriter.WriteLine("WHERE ");
                textWriter.Write(FilterDefinition);
            }
            textWriter.WriteLine(";");
            textWriter.WriteLine("GO");
        }

        internal static CreateIndex FromSQL(Sys.Table table, Sys.Index index)
        {
            return new CreateIndex
            {
                ObjectIdentitifer = SqlUtil.GetQuotedObjectIdentifierString(table.name, table.Schema.name),
                Name = index.name,
                IsUnique = index.is_unique,
                IsClusterd = index.type_desc == "CLUSTERED",
                FilterDefinition = index.has_filter ? index.filter_definition : "",
                KeyColumns = index.Columns.Where(x => !x.is_included_column).OrderBy(x => x.key_ordinal).Select(x => "[" + x.Column.Name + "]" + (x.is_descending_key ? " DESC" : "")),
                IncludedColumns = index.Columns.Where(x => x.is_included_column).OrderBy(x => x.key_ordinal).Select(x => "[" + x.Column.Name + "]" + (x.is_descending_key ? " DESC" : "")),
            };
        }
    }
}
