namespace Horton.MigrationGenerator.Sys
{
    public class CheckConstraint
    {
        public string name { get; set; }
        public bool is_system_named { get; set; }

        public int parent_object_id { get; set; }
        public string type_desc { get; set; }

        public int parent_column_id { get; set; }

        public string definition { get; set; }
    }
}
