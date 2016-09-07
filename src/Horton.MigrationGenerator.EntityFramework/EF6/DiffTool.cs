using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Dapper;
using Horton.MigrationGenerator.DDL;
using Horton.MigrationGenerator.Sys;

namespace Horton.MigrationGenerator.EF6
{
    public class DiffTool
    {
        public DiffTool(ObjectContext sourceObjectContext, DbConnection targetConnection)
        {
            _sourceObjectContext = sourceObjectContext;
            _targetConnection = targetConnection;
        }

        private readonly ObjectContext _sourceObjectContext;
        private readonly DbConnection _targetConnection;

        public IList<AbstractDatabaseChange> FindChanges()
        {
            var entityConnection = (EntityConnection)_sourceObjectContext.Connection;
            var storeItemCollection = (StoreItemCollection)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
            var objectItemsCollection = (ObjectItemCollection)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.OSpace);
            var mappingSpace = (EntityContainerMapping)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.CSSpace)[0];
            var f = mappingSpace.EntitySetMappings.First();

            var newSchemas = CheckForNewSchemas(storeItemCollection);
            var tableChanges = CheckForTableChanges(storeItemCollection);

            return newSchemas
                .Concat(tableChanges)
                .ToList();
        }

        private string TryGetEntityName(EntitySet storeEntitySet)
        {
            var entityConnection = (EntityConnection)_sourceObjectContext.Connection;
            var mappings = entityConnection.GetMetadataWorkspace().GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings;
            var mapping = mappings.SingleOrDefault(m => m.EntityTypeMappings.Single().Fragments.Single().StoreEntitySet == storeEntitySet);
            return mapping == null ? storeEntitySet.Name : mapping.EntityTypeMappings.Single().EntityType.FullName;
        }

        private IEnumerable<AbstractDatabaseChange> CheckForTableChanges(StoreItemCollection storeItemCollection)
        {
            foreach (var container in storeItemCollection.GetItems<EntityContainer>())
            {
                var entitySets = container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name);
                foreach (var entitySet in entitySets)
                {
                    var objectIdentifier = GetTableObjectIdentifierString(entitySet.Table, entitySet.Schema);

                    var existingObject = _targetConnection
                        .Query<dynamic>("SELECT * FROM sys.objects WHERE object_id= OBJECT_ID(@objectIdentifier)", new { objectIdentifier })
                        .SingleOrDefault();

                    if (existingObject == null)
                    {
                        // CREATE TABLE
                        var entityName = TryGetEntityName(entitySet);
                        yield return new CreateTable(objectIdentifier, entitySet.ElementType.Properties.Select(p => ColumnInfo.FromEF6(p)), "Create From Entity: " + entityName);
                    }

                    if (existingObject.type.Trim() == "V")
                    {
                        // mapped to a view - we won't scaffold views
                        continue;
                    }

                    foreach (var columnChange in CheckForNewColumns(entitySet, objectIdentifier))
                    {
                        yield return columnChange;
                    }
                }
            }
        }

        private IEnumerable<AbstractDatabaseChange> CheckForNewColumns(EntitySet entitySet, string objectIdentifier)
        {
            var existingColumns = _targetConnection
                        .Query<Sys.Column>("SELECT columns.*, types.name AS TypeName FROM sys.columns INNER JOIN sys.types ON types.user_type_id = columns.user_type_id WHERE object_id= OBJECT_ID(@objectIdentifier)", new { objectIdentifier }).ToList();

            var entityName = TryGetEntityName(entitySet);
            foreach (var property in entitySet.ElementType.Properties)
            {
                var typeName = property.TypeName;

                var isMaxLen = false;
                // Special case: the EDM treats 'nvarchar(max)' as a type name, but SQL Server treats
                // it as a type 'nvarchar' and a type qualifier.
                const string maxSuffix = "(max)";
                if (typeName.EndsWith(maxSuffix))
                {
                    typeName = typeName.Substring(0, typeName.Length - maxSuffix.Length);
                    isMaxLen = true;
                }

                var existingColumn = existingColumns.SingleOrDefault(x => x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                if (existingColumn == null)
                {
                    // new column
                    yield return new AddColumn(objectIdentifier, ColumnInfo.FromEF6(property));
                }

                if (!string.Equals(typeName, existingColumn.TypeName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property), $"Data Type Altered For [{entityName}]. EF = {typeName} DB = {existingColumn.TypeName}");
                }

                if (property.Nullable != existingColumn.is_nullable)
                {
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property), $"Nullability Altered For [{entityName}]. EF = {property.Nullable} DB = {existingColumn.is_nullable}");
                }

                // Compare data type properties (len, precision, scale)

                if (isMaxLen && existingColumn.max_length != -1 ||
                    !isMaxLen && existingColumn.max_length == -1 ||
                    !property.IsMaxLengthConstant && property.MaxLength.HasValue && (property.MaxLength != existingColumn.max_length))
                {
                    // length differs
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property), $"Data Type Max Length Altered For [{entityName}]. EF = {(isMaxLen ? "(max)" : property.MaxLength.ToString())} DB = {(existingColumn.max_length == -1 ? "(max)" : existingColumn.max_length.ToString())}");
                }

                byte? precision = property.IsPrecisionConstant ? null : property.Precision;
                byte? scale = property.IsScaleConstant ? null : property.Scale;

                if (typeName == "time")
                {
                    // Special case: EDM gives "time" a Precision value, but in SQL it's actually Scale
                    scale = precision;
                    precision = null;
                }

                if (precision.HasValue && (precision != existingColumn.precision))
                {
                    // scale differs
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property), $"Data Type Precision Altered For [{entityName}]. EF = {precision} DB = {existingColumn.precision}");
                }
                if (scale.HasValue && (scale != existingColumn.scale))
                {
                    // scale differs
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property), $"Data Type Scale Altered For [{entityName}]. EF = {scale} DB = {existingColumn.scale}");
                }
            }

            yield break;
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

        private static string GetTableObjectIdentifierString(string table, string schema)
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
    }
}
