using System;
using System.Collections.Generic;
using System.Linq;

namespace Horton.MigrationGenerator.Sys
{
    public class ForeignKey
    {
        public int object_id { get; set; }
        public string ForeignKeyName { get; set; }

        public int parent_object_id { get; set; }
        public string ParentSchemaName { get; set; }
        public string ParentObjectName { get; set; }

        public int referenced_object_id { get; set; }
        public string ReferencedSchemaName { get; set; }
        public string ReferencedObjectName { get; set; }

        public List<ForeignKeyColumn> Columns { get; } = new List<ForeignKeyColumn>();

        internal Table Parent { get; set; }
        internal Table Referenced { get; set; }

        public bool Matches(string parentTableName, string parentSchemaName, string parentColumnName, string referencedTableName, string referencedSchemaName, string referencedColumnName)
        {
            return string.Equals(ParentObjectName, parentTableName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ParentSchemaName, parentSchemaName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Columns.Single().ParentColumnName, parentColumnName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedObjectName, referencedTableName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedSchemaName, referencedSchemaName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Columns.Single().ReferencedColumnName, referencedColumnName, StringComparison.OrdinalIgnoreCase)
                ;
        }

        public const string SQL_SelectAll = @"
SELECT
    fk.*,
    ForeignKeyName = fk.name,
    ParentObjectName = po.name,
    ParentSchemaName = ps.name,
    ReferencedObjectName = ro.name,
    ReferencedSchemaName = rs.name
FROM sys.foreign_keys fk
    INNER JOIN sys.objects po ON po.object_id = fk.parent_object_id
    INNER JOIN sys.schemas ps ON ps.schema_id = po.schema_id
    INNER JOIN sys.objects ro ON ro.object_id = fk.referenced_object_id
    INNER JOIN sys.schemas rs ON rs.schema_id = ro.schema_id";
    }
}
