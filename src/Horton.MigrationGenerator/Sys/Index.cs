using System.Collections.Generic;

namespace Horton.MigrationGenerator.Sys
{
    public class Index
    {
        public int object_id { get; set; }
        public string name { get; set; }
        public int index_id { get; set; }
        public string type_desc { get; set; }
        public string constraint_type { get; set; }

        public bool is_unique { get; set; }
        public bool is_primary_key { get; set; }
        public bool is_unique_constraint { get; set; }

        public int fill_factor { get; set; }

        public bool has_filter { get; set; }
        public string filter_definition { get; set; }

        public bool is_system_named { get; set; }

        public List<IndexedColumn> Columns { get; } = new List<IndexedColumn>();
        public Table Table { get; internal set; }

        public const string SQL_SelectAll = @"
SELECT
    i.*
    , c.is_system_named
    , c.[type_desc] AS constraint_type
FROM sys.indexes i
    LEFT JOIN sys.key_constraints c ON i.object_id = c.parent_object_id AND i.index_id = c.unique_index_id
";
    }
}
