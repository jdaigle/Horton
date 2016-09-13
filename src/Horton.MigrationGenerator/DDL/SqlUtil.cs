namespace Horton.MigrationGenerator.DDL
{
    public static class SqlUtil
    {
        public static string GetQuotedObjectIdentifierString(string objectName)
        {
            return GetQuotedObjectIdentifierString(objectName, null);
        }

        public static string GetQuotedObjectIdentifierString(string objectName, string schema)
        {
            if (string.IsNullOrEmpty(schema))
            {
                return $"[{objectName}]";
            }
            else
            {
                return $"[{schema}].[{objectName}]";
            }
        }
    }
}
