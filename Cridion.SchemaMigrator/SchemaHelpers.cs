using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data.Sql;
using System.Data.SqlClient;

namespace Cridion.SchemaMigrator
{
    public class SchemaHelpers
    {
        public static Boolean ContainsSchemaInfo(String ConnectionString)
        {
            Server _Server = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(ConnectionString)));
            _Server.ConnectionContext.LockTimeout = 0;

            if (!_Server.Databases.Contains(_Server.ConnectionContext.DatabaseName))
                throw new Exception("Database: " + _Server.ConnectionContext.DatabaseName + " not found.");

            Database _Database = _Server.Databases[_Server.ConnectionContext.DatabaseName];

            Boolean contains = _Database.Tables.Contains("schema_info");

            return contains;
        }

        public static int GetVersionNumber(String ConnectionString)
        {
            Server _Server = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(ConnectionString)));

            if (!_Server.Databases.Contains(_Server.ConnectionContext.DatabaseName))
                throw new Exception("Database: " + _Server.ConnectionContext.DatabaseName + " not found.");

            Database _Database = _Server.Databases[_Server.ConnectionContext.DatabaseName];

            if (!_Database.Tables.Contains("schema_info"))
            {
                Table schema_info = new Table(_Database, "schema_info");
                Column version = new Column(schema_info, "version", DataType.Int);
                version.Nullable = false;
                schema_info.Columns.Add(version);
                schema_info.Create();

                using (SqlConnection SqlConnection = new SqlConnection(ConnectionString))
                {
                    SqlCommand insert = new SqlCommand("INSERT INTO schema_info VALUES (0)", SqlConnection);
                    SqlConnection.Open();
                    insert.ExecuteNonQuery();
                    SqlConnection.Close();
                }
            }

            int versionNumber = -1;
            using (SqlConnection SqlConnection = new SqlConnection(ConnectionString))
            {
                SqlCommand getVersion = new SqlCommand("SELECT version FROM schema_info", SqlConnection);
                SqlConnection.Open();
                versionNumber = (Int32)getVersion.ExecuteScalar();
                SqlConnection.Close();
            }
            return versionNumber;
        }

        public static void SetVersionNumber(int version, String ConnectionString)
        {
            GetVersionNumber(ConnectionString);
            using (SqlConnection SqlConnection = new SqlConnection(ConnectionString))
            {
                SqlCommand insert = new SqlCommand("UPDATE schema_info SET version = " + version.ToString(), SqlConnection);
                SqlConnection.Open();
                insert.ExecuteNonQuery();
                SqlConnection.Close();
            }
        }

        public static void UpdateSchemaVersionTable(String ConnectionString)
        {
            int versionNumber = GetVersionNumber(ConnectionString);

            Server _Server = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(ConnectionString)));

            if (!_Server.Databases.Contains(_Server.ConnectionContext.DatabaseName))
                throw new Exception("Database: " + _Server.ConnectionContext.DatabaseName + " not found.");

            Database _Database = _Server.Databases[_Server.ConnectionContext.DatabaseName];

            Table schema_info = _Database.Tables["schema_info"];
            if (!schema_info.Columns.Contains("migrationsvnurl"))
            {
                Column migrationsvnurl = new Column(schema_info, "migrationsvnurl", DataType.Text);
                migrationsvnurl.Nullable = true;
                schema_info.Columns.Add(migrationsvnurl);
                migrationsvnurl.Create();
            }
            _Server.ConnectionContext.Disconnect();
        }

        public static String GetMigrationSVNUrl(String ConnectionString)
        {
            GetVersionNumber(ConnectionString);
            UpdateSchemaVersionTable(ConnectionString);
            String migrationsvnurl = "";
            using (SqlConnection SqlConnection = new SqlConnection(ConnectionString))
            {
                SqlCommand getString = new SqlCommand("SELECT migrationsvnurl FROM schema_info", SqlConnection);
                SqlConnection.Open();
                object result = getString.ExecuteScalar();
                migrationsvnurl = (result is DBNull) ? "" : (String)result;
                SqlConnection.Close();
            }
            return migrationsvnurl;
        }

        public static void SetMigrationSVNUrl(String migrationsvnurl, String ConnectionString)
        {
            GetVersionNumber(ConnectionString);
            UpdateSchemaVersionTable(ConnectionString);
            using (SqlConnection SqlConnection = new SqlConnection(ConnectionString))
            {
                SqlCommand insert = new SqlCommand("UPDATE schema_info SET migrationsvnurl = '" + migrationsvnurl + "'", SqlConnection);
                SqlConnection.Open();
                insert.ExecuteNonQuery();
                SqlConnection.Close();
            }
        }
    }
}
