using System;
using System.Data.SqlClient;
using System.IO;

namespace SchemaMigrator
{
    public class SQLMigration : Migration
    {
        public String ScriptName { get; private set; }
        public String FilePath { get; private set; }

        public SQLMigration(String filePath, String name)
        {
            FilePath = filePath;
            ScriptName = name;
        }

        public override void Execute()
        {
            var connection = new SqlConnection(ConnectionString);

            var sql = string.Empty;
            using (FileStream strm = File.OpenRead(FilePath))
            {
                StreamReader reader = new StreamReader(strm);
                sql = reader.ReadToEnd();
            }
            var scriptLines = SqlHelpers.ParseSqlScript(sql);
            if (ScriptName.Length > 31)
                ScriptName = ScriptName.Substring(0, 31);

            SqlHelpers.ExecuteMultipleLineSqlTransaction(scriptLines, connection, ScriptName);
        }

    }
}
