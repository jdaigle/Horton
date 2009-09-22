using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;

namespace Cridion.SchemaMigrator
{
    public abstract class SMOMigration : Migration
    {


        protected void Execute(String SQLCommandString)
        {
            SqlConnection conn = new SqlConnection(DatabaseConnectionString);
            SqlCommand command = new SqlCommand(SQLCommandString, conn);
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }

        protected void ExecutreSqlScript(String ScriptFile)
        {
            FileInfo sqlScript = new FileInfo(ScriptFile);
            StreamReader reader = new StreamReader(sqlScript.OpenRead());
            String script = reader.ReadToEnd();
            reader.Close();

            Execute(script);
        }


    }
}
