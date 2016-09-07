using System;

namespace Horton.MigrationGenerator.Sys
{
    public class ForeignKey
    {
        public string ForeignKeyName { get; set; }

        public int parent_object_id { get; set; }
        public string ParentSchemaName { get; set; }
        public string ParentObjectName { get; set; }
        public string ParentColumnName { get; set; }

        public int referenced_object_id { get; set; }
        public string ReferencedSchemaName { get; set; }
        public string ReferencedObjectName { get; set; }
        public string ReferencedColumnName { get; set; }

        internal bool Matches(string parentTableName, string parentSchemaName, string parentColumnName, string referencedTableName, string referencedSchemaName, string referencedColumnName)
        {
            return string.Equals(ParentObjectName, parentTableName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ParentSchemaName, parentSchemaName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ParentColumnName, parentColumnName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedObjectName, referencedTableName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedSchemaName, referencedSchemaName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ReferencedColumnName, referencedColumnName, StringComparison.OrdinalIgnoreCase)
                ;
        }
    }
}
