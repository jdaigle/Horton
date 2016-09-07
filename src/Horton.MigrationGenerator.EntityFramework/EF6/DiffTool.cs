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

            var newSchemas = CheckForNewSchemas(storeItemCollection);
            var tableChanges = CheckForTableChanges(storeItemCollection);
            var fkChanges = CheckForNewForeignKeyConstraints(storeItemCollection);

            return newSchemas
                .Concat(tableChanges)
                .Concat(fkChanges)
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
                    var objectIdentifier = GetQuotedObjectIdentifierString(entitySet.Table, entitySet.Schema);

                    var existingObject = _targetConnection
                        .Query<Sys.Object>("SELECT * FROM sys.objects WHERE object_id= OBJECT_ID(@objectIdentifier)", new { objectIdentifier })
                        .SingleOrDefault();

                    if (existingObject == null)
                    {
                        // CREATE TABLE
                        var entityName = TryGetEntityName(entitySet);
                        yield return new CreateTable(objectIdentifier, entitySet.ElementType.Properties.Select(p => ColumnInfo.FromEF6(p, entitySet.Table)), "Create From Entity: " + entityName);
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

        private IEnumerable<AbstractDatabaseChange> CheckForNewForeignKeyConstraints(StoreItemCollection storeItemCollection)
        {
            var allFKs = _targetConnection.Query<Sys.ForeignKey>(@"
SELECT
    fkc.*,
    ForeignKeyName = fk.name,
    ParentObjectName = po.name,
    ParentSchemaName = ps.name,
    ParentColumnName = pc.name,
    ReferencedObjectName = ro.name,
    ReferencedSchemaName = rs.name,
    ReferencedColumnName = rc.name
FROM sys.foreign_key_columns fkc
    INNER JOIN sys.foreign_keys fk ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.objects po ON po.object_id = fkc.parent_object_id
    INNER JOIN sys.schemas ps ON ps.schema_id = po.schema_id
    INNER JOIN sys.columns pc ON pc.column_id = fkc.parent_column_id AND pc.object_id = fkc.parent_object_id
    INNER JOIN sys.objects ro ON ro.object_id = fkc.referenced_object_id
    INNER JOIN sys.schemas rs ON rs.schema_id = ro.schema_id
    INNER JOIN sys.columns rc ON rc.column_id = fkc.referenced_column_id AND rc.object_id = fkc.referenced_object_id").ToList();

            foreach (var container in storeItemCollection.GetItems<EntityContainer>())
            {
                foreach (var associationSet in container.BaseEntitySets.OfType<AssociationSet>().OrderBy(s => s.Name))
                {
                    var constraint = associationSet.ElementType.ReferentialConstraints.Single();
                    var principalEnd = associationSet.AssociationSetEnds[constraint.FromRole.Name];
                    var dependentEnd = associationSet.AssociationSetEnds[constraint.ToRole.Name];

                    var fkName = associationSet.Name;
                    var parentTableName = dependentEnd.EntitySet.Table;
                    var parentSchemaName = dependentEnd.EntitySet.Schema;
                    var parentColumnName = constraint.ToProperties[0].Name;
                    var referencedTableName = principalEnd.EntitySet.Table;
                    var referencedSchemaName = principalEnd.EntitySet.Schema;
                    var referencedColumnName = constraint.FromProperties[0].Name;

                    if (!allFKs.Exists(fk => fk.Matches(parentTableName, parentSchemaName, parentColumnName, referencedTableName, referencedSchemaName, referencedColumnName)))
                    {
                        yield return new AddForeignKey
                        {
                            ForeignKeyObjectIdentifier = GetQuotedObjectIdentifierString(fkName, parentSchemaName),
                            ParentObjectIdentifier = GetQuotedObjectIdentifierString(parentTableName, parentSchemaName),
                            ParentObjectColumnName = parentColumnName,
                            ReferencedObjectIdentifier = GetQuotedObjectIdentifierString(referencedTableName, referencedSchemaName),
                            ReferencedObjectColumnName = referencedColumnName,
                        };
                    }
                }
            }

            yield break;
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
                    yield return new AddColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table));
                }

                if (property.IsStoreGeneratedIdentity != existingColumn.is_identity && typeName != "uniqueidentifier")
                {
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"IDENTITY Attribute Altered For [{entityName}]. EF = {property.IsStoreGeneratedIdentity} DB = {existingColumn.is_identity}");
                }

                if (property.DefaultValue != null)
                {

                }

                if (!string.Equals(typeName, existingColumn.TypeName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Altered For [{entityName}]. EF = {typeName} DB = {existingColumn.TypeName}");
                }

                if (property.Nullable != existingColumn.is_nullable)
                {
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Nullability Altered For [{entityName}]. EF = {property.Nullable} DB = {existingColumn.is_nullable}");
                }

                // Compare data type properties (len, precision, scale)

                if (isMaxLen && existingColumn.max_length != -1 ||
                    !isMaxLen && existingColumn.max_length == -1 ||
                    !property.IsMaxLengthConstant && property.MaxLength.HasValue && (property.MaxLength != existingColumn.max_length))
                {
                    // length differs
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Max Length Altered For [{entityName}]. EF = {(isMaxLen ? "(max)" : property.MaxLength.ToString())} DB = {(existingColumn.max_length == -1 ? "(max)" : existingColumn.max_length.ToString())}");
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
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Precision Altered For [{entityName}]. EF = {precision} DB = {existingColumn.precision}");
                }
                if (scale.HasValue && (scale != existingColumn.scale))
                {
                    // scale differs
                    yield return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Scale Altered For [{entityName}]. EF = {scale} DB = {existingColumn.scale}");
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

        private static string GetQuotedObjectIdentifierString(string objectName, string schema)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return $"[{objectName}]";
            }
            else
            {
                return $"[{schema}].[{objectName}]";
            }
        }
    }
}
