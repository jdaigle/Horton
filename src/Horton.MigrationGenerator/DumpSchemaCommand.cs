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
using Horton.MigrationGenerator.Sys;

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
                        ddl.AddRange(table.Indexes.Where(x => !(x.is_primary_key || x.is_unique_constraint)).Select(x => CreateIndex.FromSQL(table, x)));
                        tables.Remove(table);
                        createdTables.Add(table);
                        continue;
                    }
                    if (table.ForeignKeys.All(fk => createdTables.Contains(fk.Referenced) || fk.IsCircularDependency))
                    {
                        var createTable = CreateTable.FromSQL(table);
                        ddl.Add(CreateTable.FromSQL(table));
                        ddl.AddRange(table.Indexes.Where(x => !(x.is_primary_key || x.is_unique_constraint)).Select(x => CreateIndex.FromSQL(table, x)));
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
                lastCount = tables.Count;
            }
            foreach (var fk in createdTables.SelectMany(t => t.ForeignKeys.Where(fk => fk.IsCircularDependency)))
            {
                ddl.Add(new AddForeignKey(new ForeignKeyInfo
                {
                    ForeignKeyObjectIdentifier = fk.ForeignKeyName,
                    ParentObjectIdentifier = SqlUtil.GetQuotedObjectIdentifierString(fk.Parent.name, fk.Parent.Schema.name),
                    ParentObjectColumns = fk.Columns.OrderBy(c => c.constraint_object_id).Select(c => c.ParentColumnName),
                    ReferencedObjectIdentifier = SqlUtil.GetQuotedObjectIdentifierString(fk.Referenced.name, fk.Referenced.Schema.name),
                    ReferencedObjectColumns = fk.Columns.OrderBy(c => c.constraint_object_id).Select(c => c.ReferencedColumnName),
                }, "This FK represents the nullable side of a circular dependency."));
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
                var fkCols = connection.Query<Sys.ForeignKeyColumn>(Sys.ForeignKeyColumn.SQL_SelectAll).ToLookup(x => x.constraint_object_id);
                var fks = connection.Query<Sys.ForeignKey>(Sys.ForeignKey.SQL_SelectAll).ToList();
                foreach (var fk in fks)
                {
                    fk.Columns.AddRange(fkCols[fk.object_id]);
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
                    }
                }
                Program.PrintLine("...done.");

                Program.Print("Query columns and table constraints...");
                foreach (var table in tables.Values)
                {
                    table.Schema = schemas[table.schema_id];

                    table.TableCheckConstraints.AddRange(connection.Query<Sys.CheckConstraint>("SELECT * FROM sys.check_constraints WHERE parent_column_id = 0 AND parent_object_id= @object_id", new { table.object_id }));

                    table.Columns.AddRange(connection.Query<Sys.Column>(Sys.Column.SQL_SelectAll + " WHERE c.object_id= @object_id", new { table.object_id }));

                    foreach (var fk in table.ForeignKeys)
                    {
                        foreach (var col in fk.Columns)
                        {
                            col.ParentColumn = table.Columns.Single(x => x.column_id == col.parent_column_id);
                        }
                    }
                    foreach (var fk in table.OutboundForeignKeys)
                    {
                        foreach (var col in fk.Columns)
                        {
                            col.ReferencedColumn = table.Columns.Single(x => x.column_id == col.referenced_column_id);
                        }
                    }
                    foreach (var idx in table.Indexes)
                    {
                        foreach (var col in idx.Columns)
                        {
                            col.Column = table.Columns.Single(x => x.column_id == col.column_id);
                        }
                    }
                }
                Program.PrintLine("...done.");

                Program.Print("Analyzing FK dependency graph...");
                foreach (var table in tables.Values)
                {
                    foreach (var fk in table.ForeignKeys)
                    {
                        fk.IsCircularDependency = IsCircularDependency(table, fk);
                    }
                }
                Program.PrintLine("...done.");

                connection.Close();

                return tables.Values.OrderBy(x => x.create_date).ThenBy(x => x.Schema.name).ThenBy(x => x.name).ToList();
            }
        }

        private List<ForeignKey> visitedFK = new List<ForeignKey>();

        private bool IsCircularDependency(Table rootTable, ForeignKey fk)
        {
            visitedFK.Clear();
            return IsCircularDependencyRecurisve(rootTable, fk);
        }

        private bool IsCircularDependencyRecurisve(Table rootTable, ForeignKey fk)
        {
            if (visitedFK.Contains(fk))
            {
                return false;
            }
            visitedFK.Add(fk);
            if (fk.Referenced == rootTable)
            {
                return true;
            }
            return fk.Referenced.ForeignKeys.Any(_fk => IsCircularDependencyRecurisve(rootTable, _fk));
        }
    }
}
