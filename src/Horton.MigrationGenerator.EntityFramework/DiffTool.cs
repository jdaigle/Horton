using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Dapper;

namespace Horton.MigrationGenerator.EF6
{
    public class DiffTool
    {
        private readonly ObjectContext _sourceObjectContext;
        private readonly DbConnection _targetConnection;

        public IList<AbstractDatabaseChange> FindChanges()
        {
            var entityConnection = (EntityConnection)_sourceObjectContext.Connection;
            var storeItemCollection = (StoreItemCollection)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);

            var newSchemas = CheckForNewSchemas(storeItemCollection);
            var tableChanges = CheckForTableChanges(storeItemCollection);

            return newSchemas
                .Concat(tableChanges)
                .ToList();
        }

        private IEnumerable<AbstractDatabaseChange> CheckForTableChanges(StoreItemCollection storeItemCollection)
        {
            foreach (var container in storeItemCollection.GetItems<EntityContainer>())
            {
                var entitySets = container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name);
                foreach (var entitySet in entitySets)
                {
                    var tableName = entitySet.Table;
                    var schemaName = entitySet.Schema;
                    var objectIdentifier = GetTableObjectIdentifierString(entitySet.Table, entitySet.Schema);

                    var existingTable = _targetConnection
                        .Query<dynamic>("SELECT * FROM sys.tables WHERE object_id= OBJECT_ID('@objectIdentifier')", new { objectIdentifier })
                        .SingleOrDefault();

                    if (existingTable == null)
                    {
                        // CREATE TABLE
                        yield return GetCreateTable(entitySet);
                    }
                }
            }
        }

        private CreateTable GetCreateTable(EntitySet entitySet)
        {
            var createTable = new CreateTable(entitySet.Table, entitySet.Schema);

            foreach (var property in entitySet.ElementType.Properties)
            {
                var typeUsage = property.TypeUsage;
                createTable.Columns.Add(new ColumnInfo(property.Name, typeUsage.EdmType.Name)
                {
                    IsNullable = property.Nullable,
                    IsIdentity = property.IsStoreGeneratedIdentity,

                    IsUnicode = property.IsUnicodeConstant ? null : property.IsUnicode,
                    IsFixedLength = property.IsFixedLengthConstant ? null : property.IsFixedLength,
                    MaxLength= property.IsMaxLengthConstant ? null : property.MaxLength,
                    IsMaxLength = property.IsMaxLength,
                    Precision = property.IsPrecisionConstant ? null : property.Precision,
                    Scale = property.IsScaleConstant ? null : property.Scale,
                });
            }

            return createTable;
        }

        private string GetTableObjectIdentifierString(string table, string schema)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return $"[{table}]";
            }
            else
            {
                return $"[{schema}].[{table}]";
            }
        }

        private IEnumerable<AbstractDatabaseChange> CheckForNewSchemas(StoreItemCollection storeItemCollection)
        {
            var existingSchemas = _targetConnection.Query<string>("SELECT name FROM sys.schemas");

            var modelSchemas =
            storeItemCollection.GetItems<EntityContainer>()
                               .SelectMany(c => c.BaseEntitySets.OfType<EntitySet>().Select(s => s.Schema))
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .OrderBy(x => x);

            foreach (var schemaName in modelSchemas)
            {
                if (!existingSchemas.Contains(schemaName, StringComparer.OrdinalIgnoreCase))
                {
                    yield return new CreateSchema(schemaName);
                }
            }
        }
    }
}
