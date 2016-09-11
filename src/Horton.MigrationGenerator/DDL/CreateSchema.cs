using System.CodeDom.Compiler;

namespace Horton.MigrationGenerator.DDL
{
    public sealed class CreateSchema : AbstractDatabaseChange
    {
        public CreateSchema(string schemaName)
        {
            SchemaName = schemaName;
        }

        private string SchemaName { get; }

        public override void AppendDDL(IndentedTextWriter textWriter)
        {
            textWriter.WriteLine($"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{SchemaName}')");
            textWriter.WriteLine($"EXEC sys.sp_executesql N'CREATE SCHEMA [{SchemaName}]';");
            textWriter.WriteLine("GO");
        }
    }
}
