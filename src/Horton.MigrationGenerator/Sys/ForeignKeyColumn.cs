namespace Horton.MigrationGenerator.Sys
{
    public class ForeignKeyColumn
    {
        public int constraint_object_id { get; set; }
        public int constraint_column_id { get; set; }

        public int parent_object_id { get; set; }
        public int parent_column_id { get; set; }
        public string ParentColumnName { get; set; }

        public int referenced_object_id { get; set; }
        public int referenced_column_id { get; set; }
        public string ReferencedColumnName { get; set; }

        public const string SQL_SelectAll = @"
SELECT
    fkc.*,
    ParentColumnName = pc.name,
    ReferencedColumnName = rc.name
FROM sys.foreign_key_columns fkc
    INNER JOIN sys.columns pc ON pc.column_id = fkc.parent_column_id AND pc.object_id = fkc.parent_object_id
    INNER JOIN sys.columns rc ON rc.column_id = fkc.referenced_column_id AND rc.object_id = fkc.referenced_object_id";
    }
}
