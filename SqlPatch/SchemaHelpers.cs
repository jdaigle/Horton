using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SqlPatch
{
    public class SchemaHelpers
    {
        public static string CreateConnectionString()
        {
            var connectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = Configuration.World.Server;
            connectionStringBuilder.InitialCatalog = Configuration.World.Database;
            connectionStringBuilder.IntegratedSecurity = Configuration.World.IntegratedSecurity;
            if (!Configuration.World.IntegratedSecurity)
            {
                connectionStringBuilder.UserID = Configuration.World.Username;
                connectionStringBuilder.Password = Configuration.World.Password;
            }
            return connectionStringBuilder.ToString();
        }

        public static void EnsureSchemaInfoTable()
        {
            var query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='schema_info'";
            var createtable = "CREATE TABLE schema_info ( version int NOT NULL, migration_script varchar(255) NOT NULL )";
            using (var connection = new SqlConnection(CreateConnectionString()))
            {
                var queryCommand = new SqlCommand(query, connection);
                connection.Open();
                var result = queryCommand.ExecuteScalar();
                if (result == null || (int)result != 1)
                {
                    var createCommand = new SqlCommand(createtable, connection);
                    createCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static IList<int> GetVerionNumbers()
        {
            var versions = new List<int>();
            var query = "SELECT version FROM [schema_info] ORDER BY version ASC";
            using (var connection = new SqlConnection(CreateConnectionString()))
            {
                var queryCommand = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        versions.Add(reader.GetInt32(0));
                    }
                }
                connection.Close();
            }
            return versions.OrderBy(x => x).ToList();
        }

        public static void InsertVersion(int version, string migrationScript)
        {
            var insert = "INSERT INTO schema_info (version, migration_script) VALUES (" + version + ", '" + migrationScript + "')";
            using (var connection = new SqlConnection(CreateConnectionString()))
            {
                var cmd = new SqlCommand(insert, connection);
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
