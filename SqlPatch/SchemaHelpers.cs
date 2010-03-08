using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlPatch {
    public class SchemaHelpers {
        public static string CreateConnectionString() {
            var connectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = Configuration.World.Server;
            connectionStringBuilder.InitialCatalog = Configuration.World.Database;
            connectionStringBuilder.IntegratedSecurity = Configuration.World.IntegratedSecurity;
            if (!Configuration.World.IntegratedSecurity) {
                connectionStringBuilder.UserID = Configuration.World.Username;
                connectionStringBuilder.Password = Configuration.World.Password;
            }
            return connectionStringBuilder.ToString();
        }

        public static void EnsureSchemaInfoTable() {
            var query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='schema_info'";
            var createtable = "CREATE TABLE schema_info ( id uniqueidentifier NOT NULL, contentHash uniqueidentifier NOT NULL, type int NOT NULL, applied datetime NOT NULL, fileName varchar(255) NULL, CONSTRAINT PK_schema_info PRIMARY KEY CLUSTERED (id) )";
            using (var connection = new SqlConnection(CreateConnectionString())) {
                var queryCommand = new SqlCommand(query, connection);
                connection.Open();
                var result = queryCommand.ExecuteScalar();
                if (result == null || (int)result != 1) {
                    Logger.WriteLine("Creating schema_info table.");
                    var createCommand = new SqlCommand(createtable, connection);
                    createCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static IList<AppliedScript> GetScripts() {
            var appliedScripts = new List<AppliedScript>();
            var query = "SELECT id, contentHash, type, applied, fileName FROM schema_info ORDER BY applied ASC";
            using (var connection = new SqlConnection(CreateConnectionString())) {
                var queryCommand = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = queryCommand.ExecuteReader()) {
                    while (reader.Read()) {
                        var id = reader.GetGuid(0);
                        var hash = reader.GetGuid(1);
                        var type = (ScriptType)reader.GetInt32(2);
                        var applied = reader.GetDateTime(3);
                        var fileName = reader.GetString(4);
                        appliedScripts.Add(new AppliedScript(id, hash, applied, fileName, type));
                    }
                }
                connection.Close();
            }
            return appliedScripts;
        }

        public static void InsertScript(IScript script) {
            var upsert = "UPDATE schema_info SET contentHash = @P2, applied = @P4 WHERE id = @P1 " +
                         "IF @@ROWCOUNT = 0 INSERT INTO schema_info (id, contentHash, type, applied, fileName) VALUES (@P1, @P2, @P3, @P4, @P5)";
            using (var connection = new SqlConnection(CreateConnectionString())) {
                var cmd = new SqlCommand(upsert, connection);
                cmd.Parameters.Add(new SqlParameter("@P1", script.Id));
                cmd.Parameters.Add(new SqlParameter("@P2", script.ContentHash));
                cmd.Parameters.Add(new SqlParameter("@P3", (int)script.Type));
                cmd.Parameters.Add(new SqlParameter("@P4", DateTime.UtcNow));
                cmd.Parameters.Add(new SqlParameter("@P5", script.FileName));
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
