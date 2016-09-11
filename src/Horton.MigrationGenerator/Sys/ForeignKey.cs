using System;

namespace Horton.MigrationGenerator.Sys
{
    public class ForeignKey
    {
        public int constraint_object_id { get; set; }
        public string ForeignKeyName { get; set; }

        public int parent_object_id { get; set; }
        public int parent_column_id { get; set; }
        public string ParentSchemaName { get; set; }
        public string ParentObjectName { get; set; }
        public string ParentColumnName { get; set; }

        public int referenced_object_id { get; set; }
        public int referenced_column_id { get; set; }
        public string ReferencedSchemaName { get; set; }
        public string ReferencedObjectName { get; set; }
        public string ReferencedColumnName { get; set; }

        internal Table Parent { get; set; }
        internal Table Referenced { get; set; }

        public bool Matches(string parentTableName, string parentSchemaName, string parentColumnName, string referencedTableName, string referencedSchemaName, string referencedColumnName)
        {
            return string.Equals(ParentObjectName, parentTableName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ParentSchemaName, parentSchemaName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ParentColumnName, parentColumnName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedObjectName, referencedTableName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedSchemaName, referencedSchemaName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedColumnName, referencedColumnName, StringComparison.OrdinalIgnoreCase)
                ;
        }

        public const string SQL_SelectAll = @"
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
    INNER JOIN sys.columns rc ON rc.column_id = fkc.referenced_column_id AND rc.object_id = fkc.referenced_object_id";

    }
}
