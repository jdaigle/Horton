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
                    var objectIdentifier = SqlUtil.GetQuotedObjectIdentifierString(entitySet.Table, entitySet.Schema);

                    var existingObject = _targetConnection
                        .Query<Sys.Object>("SELECT * FROM sys.objects WHERE object_id= OBJECT_ID(@objectIdentifier)", new { objectIdentifier })
                        .SingleOrDefault();

                    if (existingObject == null)
                    {
                        // CREATE TABLE
                        var entityName = TryGetEntityName(entitySet);
                        yield return new CreateTable(objectIdentifier, entitySet.ElementType.Properties.Select(p => ColumnInfo.FromEF6(p, entitySet.Table)), "Create From Entity: " + entityName);
                        continue;
                    }

                    if (existingObject.type.Trim() == "V")
                    {
                        // mapped to a view - we won't scaffold views
                        continue;
                    }

                    var existingColumns = _targetConnection
                        .Query<Sys.Column>("SELECT columns.*, types.name AS TypeName FROM sys.columns INNER JOIN sys.types ON types.user_type_id = columns.user_type_id WHERE object_id= OBJECT_ID(@objectIdentifier)", new { objectIdentifier }).ToList();

                    foreach (var property in entitySet.ElementType.Properties)
                    {
                        var columnChange = CheckColumn(property, existingColumns, entitySet, objectIdentifier);
                        if (columnChange != null)
                        {
                            yield return columnChange;
                        }
                    }
                }
            }
        }

        private IEnumerable<AbstractDatabaseChange> CheckForNewForeignKeyConstraints(StoreItemCollection storeItemCollection)
        {
            var fkCols = _targetConnection.Query<Sys.ForeignKeyColumn>(Sys.ForeignKeyColumn.SQL_SelectAll).ToLookup(x => x.constraint_object_id);
            var allFKs = _targetConnection.Query<Sys.ForeignKey>(Sys.ForeignKey.SQL_SelectAll).ToList();
            foreach (var fk in allFKs)
            {
                fk.Columns.AddRange(fkCols[fk.object_id]);
            }

            foreach (var container in storeItemCollection.GetItems<EntityContainer>())
            {
                foreach (var associationSet in container.BaseEntitySets.OfType<AssociationSet>().OrderBy(s => s.Name))
                {
                    var constraint = associationSet.ElementType.ReferentialConstraints.Single();
                    var principalEnd = associationSet.AssociationSetEnds[constraint.FromRole.Name];
                    var dependentEnd = associationSet.AssociationSetEnds[constraint.ToRole.Name];
                    var deleteBehavior = constraint.FromRole.DeleteBehavior;

                    var parentTableName = dependentEnd.EntitySet.Table;
                    var parentSchemaName = dependentEnd.EntitySet.Schema;
                    var parentColumnName = constraint.ToProperties.Single().Name;
                    var referencedTableName = principalEnd.EntitySet.Table;
                    var referencedSchemaName = principalEnd.EntitySet.Schema;
                    var referencedColumnName = constraint.FromProperties.Single().Name;
                    var fkName = "FK_" + parentTableName + "_" + referencedTableName + "_" + parentColumnName;

                    if (!allFKs.Exists(fk => fk.Matches(parentTableName, parentSchemaName, parentColumnName, referencedTableName, referencedSchemaName, referencedColumnName)))
                    {
                        yield return new AddForeignKey(new ForeignKeyInfo
                        {
                            QuotedForeignKeyName = SqlUtil.GetQuotedObjectIdentifierString(fkName),
                            ParentObjectIdentifier = SqlUtil.GetQuotedObjectIdentifierString(parentTableName, parentSchemaName),
                            ParentObjectColumns = new[] { parentColumnName },
                            ReferencedObjectIdentifier = SqlUtil.GetQuotedObjectIdentifierString(referencedTableName, referencedSchemaName),
                            ReferencedObjectColumns = new[] { referencedColumnName },
                            CascadeDelete = deleteBehavior == OperationAction.Cascade,
                        }, "");
                    }
                }
            }
        }

        private AbstractDatabaseChange CheckColumn(EdmProperty property, IEnumerable<Sys.Column> existingColumns, EntitySet entitySet, string objectIdentifier)
        {
            var entityName = TryGetEntityName(entitySet);
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
            // Special case: XML is always "max"
            if (typeName == "xml")
            {
                isMaxLen = true;
            }

            var existingColumn = existingColumns.SingleOrDefault(x => x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
            if (existingColumn == null)
            {
                // new column
                return new AddColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table));
            }

            if (property.IsUnicode == true && existingColumn.max_length > 0)
            {
                // unicode (nchar,nvarchar) are storaged at double the length
                existingColumn.max_length = (short)(existingColumn.max_length / 2);
            }

            if (property.IsStoreGeneratedIdentity != existingColumn.is_identity && typeName != "uniqueidentifier")
            {
                return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"IDENTITY Attribute Altered For \"{entityName}.{property.Name}\". SQL Type: {existingColumn.ToInfoString()}");
            }

            if (property.DefaultValue != null)
            {

            }

            if (!string.Equals(typeName, existingColumn.TypeName, StringComparison.OrdinalIgnoreCase))
            {
                return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Altered For \"{entityName}.{property.Name}\". SQL Type: {existingColumn.ToInfoString()}");
            }

            if (property.Nullable != existingColumn.is_nullable)
            {
                return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Nullability Altered For \"{entityName}.{property.Name}\". SQL Type: {existingColumn.ToInfoString()}");
            }

            // Compare data type properties (len, precision, scale)
            if (isMaxLen && existingColumn.max_length != -1 ||
                !isMaxLen && existingColumn.max_length == -1 ||
                !property.IsMaxLengthConstant && property.MaxLength.HasValue && (property.MaxLength != existingColumn.max_length))
            {
                // length differs
                return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Max Length Altered For \"{entityName}.{property.Name}\". SQL Type: {existingColumn.ToInfoString()}");
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
                return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Precision Altered For \"{entityName}.{property.Name}\". SQL Type: {existingColumn.ToInfoString()}");
            }
            if (scale.HasValue && (scale != existingColumn.scale))
            {
                // scale differs
                return new AlterColumn(objectIdentifier, ColumnInfo.FromEF6(property, entitySet.Table), $"Data Type Scale Altered For \"{entityName}.{property.Name}\". SQL Type: {existingColumn.ToInfoString()}");
            }

            return null;
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
