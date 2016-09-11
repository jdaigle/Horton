namespace Horton.MigrationGenerator.Sys
{
    public class Column
    {
        public int object_id { get; set; }
        public string Name { get; set; }
        public int column_id { get; set; }
        public byte system_type_id { get; set; }
        public int user_type_id { get; set; }
        public string TypeName { get; set; }
        public short max_length { get; set; }
        public byte precision { get; set; }
        public byte scale { get; set; }
        public bool is_nullable { get; set; }
        public bool is_identity { get; set; }
        public bool is_computed { get; set; }
        public bool is_rowguidcol { get; set; }
        public bool is_user_defined { get; set; }

        public int default_object_id { get; set; }
        public string default_name { get; set; }
        public string default_definition { get; set; }
        public bool default_is_system_named { get; set; }

        public string check_constraint_name { get; set; }
        public string check_constraint_definition { get; set; }
        public bool check_constraint_is_system_named { get; set; }

        public string ToInfoString()
        {
            return $"[{TypeName}],Len({max_length}),P({precision}),S({scale}) {(is_nullable ? "NULL" : "NOT NULL")}";
        }

        public const string SQL_SelectAll = @"
SELECT 
    c.*
    , t.name AS TypeName
    , t.is_user_defined
    , df.name AS default_name
    , df.[definition] AS default_definition
    , df.is_system_named AS default_is_system_named
    , chk.name AS check_constraint_name
    , chk.[definition] AS check_constraint_definition
    , chk.is_system_named AS check_constraint_is_system_named
FROM sys.columns c
    INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
    LEFT JOIN sys.default_constraints df ON df.object_id = c.default_object_id
    LEFT JOIN sys.check_constraints chk ON chk.parent_object_id = c.object_id AND chk.parent_column_id = c.column_id
";
    }
}
