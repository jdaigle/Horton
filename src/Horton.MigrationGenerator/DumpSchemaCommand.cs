using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using Horton.MigrationGenerator.DDL;

namespace Horton.MigrationGenerator
{
    public class DumpSchemaCommand : HortonCommand
    {
        public override string Name { get { return "DUMP-SCHEMA"; } }
        public override string Description { get { return "TODO"; } }

        public override void Execute(HortonOptions options)
        {
            Program.PrintLine("=== Dump Schema ===");
            Program.PrintLine();

            var tables = LoadSchema(options);
            Program.PrintLine();

            var ddlList = FindChanges(tables);
            var sb = new StringBuilder();
            var textWriter = new IndentedTextWriter(new StringWriter(sb));
            foreach (var ddl in ddlList)
            {
                ddl.AppendDDL(textWriter);
                textWriter.WriteLine();
                textWriter.WriteLine();
            }
            if (sb.Length > 0)
            {
                File.WriteAllText(Path.Combine(options.MigrationsDirectoryPath, "000_BaseSchema.sql"), sb.ToString());
            }
        }

        public IList<AbstractDatabaseChange> FindChanges(List<Sys.Table> tables)
        {
            var ddl = new List<AbstractDatabaseChange>();

            var createdTables = new List<Sys.Table>();

            // if no circular dependencies exist, then this function _should_ complete
            var lastCount = tables.Count;
            while (tables.Count > 0)
            {
                foreach (var table in tables.ToList())
                {
                    if (table.ForeignKeys.Count == 0)
                    {
                        ddl.Add(CreateTable.FromSQL(table));
                        tables.Remove(table);
                        createdTables.Add(table);
                        continue;
                    }
                    if (table.ForeignKeys.All(fk => createdTables.Contains(fk.Referenced)))
                    {
                        var createTable = CreateTable.FromSQL(table);
                        ddl.Add(CreateTable.FromSQL(table));
                        tables.Remove(table);
                        createdTables.Add(table);
                        continue;
                    }
                }
                // safety check, we _must_ decrement on each loop, otherwise there
                // is a problem if the build graph (possible circular reference)
                if (tables.Count == lastCount)
                {
                    throw new Exception("The depedency graph could not resolve the build order.");
                }
            }
            return ddl;
        }

        private List<Sys.Table> LoadSchema(HortonOptions options)
        {
            using (var connection = new SqlConnection(options.CreateConnectionString()))
            {
                connection.Open();

                Program.Print("Querying schemas...");
                var schemas = connection.Query<Sys.Schema>("SELECT * FROM sys.schemas").ToDictionary(x => x.schema_id);
                Program.PrintLine("...done.");

                Program.Print("Querying tables...");
                var tables = connection.Query<Sys.Table>("SELECT * FROM sys.tables").ToDictionary(x => x.object_id);
                Program.PrintLine("...done.");

                Program.Print("Querying foreign keys...");
                var fks = connection.Query<Sys.ForeignKey>(Sys.ForeignKey.SQL_SelectAll).ToList();
                foreach (var fk in fks)
                {
                    fk.Parent = tables[fk.parent_object_id];
                    fk.Parent.ForeignKeys.Add(fk);
                    fk.Referenced = tables[fk.referenced_object_id];
                    fk.Referenced.OutboundForeignKeys.Add(fk);
                }
                Program.PrintLine("...done.");

                Program.Print("Querying indexes...");
                var indexedColumns = connection.Query<Sys.IndexedColumn>("SELECT * FROM sys.index_columns").ToLookup(x => Tuple.Create(x.object_id, x.index_id));
                var indexes = connection.Query<Sys.Index>(Sys.Index.SQL_SelectAll).ToList();
                foreach (var index in indexes)
                {
                    index.Columns.AddRange(indexedColumns[Tuple.Create(index.object_id, index.index_id)]);
                    if (tables.ContainsKey(index.object_id))
                    {
                        index.Table = tables[index.object_id];
                        index.Table.Indexes.Add(index);
                        if (index.Table.Columns.Count == 0)
                        {
                            index.Table.Columns.AddRange(connection.Query<Sys.Column>(Sys.Column.SQL_SelectAll + " WHERE c.object_id= @object_id", new { index.Table.object_id }));
                        }
                        foreach (var column in index.Columns)
                        {
                            column.Column = index.Table.Columns.Single(x => x.column_id == column.column_id);
                        }
                    }
                }
                Program.PrintLine("...done.");

                Program.Print("Analyzing dependency graph...");
                foreach (var table in tables.Values)
                {
                    table.Schema = schemas[table.schema_id];
                    table.ForeignKeyDeptch = FKDepthRecursive(table);
                    if (table.Columns.Count == 0)
                    {
                        table.Columns.AddRange(connection.Query<Sys.Column>(Sys.Column.SQL_SelectAll + " WHERE c.object_id= @object_id", new { table.object_id }));
                    }
                    if (FollowFK(table).Contains(table))
                    {
                        Debug.Fail("Circular Reference Detected!");
                    }
                }
                Program.PrintLine("...done.");

                connection.Close();

                return tables.Values.OrderBy(x => x.create_date).ThenBy(x => x.name).ToList();
            }
        }

        public static int FKDepthRecursive(Sys.Table root)
        {
            if (root.ForeignKeys.Count == 0)
            {
                return 0;
            }
            return root.ForeignKeys.Max(fk => FKDepthRecursive(fk.Referenced)) + 1;
        }

        public static IEnumerable<Sys.Table> FollowFK(Sys.Table root)
        {
            foreach (var fk in root.ForeignKeys)
            {
                yield return fk.Referenced;

                foreach (var child in FollowFK(fk.Referenced))
                {
                    yield return child;
                }
            }
        }
    }
}
