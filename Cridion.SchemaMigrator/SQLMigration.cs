using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;

namespace Cridion.SchemaMigrator
{
    public class SQLMigration : Migration
    {

        private String _SQLScriptName;
	    public String SQLScriptName
	    {
	      get { return _SQLScriptName;}
	    }

        private String _FullFileName;
        public String FullFileName
        {
            get { return _FullFileName; }
        }

        public SQLMigration(String FullFileName, String Name)
        {
            _FullFileName = FullFileName;
            _SQLScriptName = Name;
        }


        public void ExecuteScript()
        {
            SqlConnection connection = new SqlConnection(Configuration.CurrentConfig.ConnectionString);

            string sql = "";

            using (FileStream strm = File.OpenRead(_FullFileName))
            {
                StreamReader reader = new StreamReader(strm);
                sql = reader.ReadToEnd();
            }

            string[] lines = SqlHelpers.ParseSqlScript(sql);

            if (_SQLScriptName.Length > 31)
                _SQLScriptName = _SQLScriptName.Substring(0, 31);

            try
            {
                SqlHelpers.ExecuteMultipleLineSqlTransaction(lines, connection, _SQLScriptName);
            }
            catch (SqlException e)
            {
                throw new MigrationException(this, "", "Script Failed, Migration Rolling Back", e);
            }

        }

        public override void Up()
        {
            ExecuteScript();
        }

        public override void Down()
        {
            ExecuteScript();
        }


        public override string ToString()
        {
            return _SQLScriptName;
        }
            
    }
}
