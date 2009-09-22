using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data.Sql;
using System.Data.SqlClient;


namespace Cridion.SchemaMigrator
{
    public class SchemaManager
    {        

        public static Boolean ContainsSchemaInfo()
        {
            return SchemaHelpers.ContainsSchemaInfo(Configuration.CurrentConfig.ConnectionString);           
        }

        public static int GetVersionNumber()
        {
            return SchemaHelpers.GetVersionNumber(Configuration.CurrentConfig.ConnectionString);
        }

        public static void SetVersionNumber(int version)
        {
            SchemaHelpers.SetVersionNumber(version, Configuration.CurrentConfig.ConnectionString);
        }

        public static void UpdateSchemaVersionTable()
        {
            SchemaHelpers.UpdateSchemaVersionTable(Configuration.CurrentConfig.ConnectionString);
        }

        public static String GetMigrationSVNUrl()
        {
            return SchemaHelpers.GetMigrationSVNUrl(Configuration.CurrentConfig.ConnectionString);
        }

        public static void SetMigrationSVNUrl(String migrationsvnurl)
        {
            SchemaHelpers.SetMigrationSVNUrl(migrationsvnurl, Configuration.CurrentConfig.ConnectionString);
        }


    }
}
