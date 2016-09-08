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

        public string ToInfoString()
        {
            return $"[{TypeName}],Len({max_length}),P({precision}),S({scale}) {(is_nullable ? "NULL" : "NOT NULL")}";
        }
    }
}
