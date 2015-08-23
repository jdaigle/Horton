using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace SqlMigrate {
    public class ScriptEngine {

        private readonly IList<ScriptFile> scripts;
        private readonly string connectionString;

        public ScriptEngine(IList<ScriptFile> scripts, string connectionString) {
            this.scripts = scripts;
            this.connectionString = connectionString;
        }

        public void Execute() {
            var connection = new SqlConnection(connectionString);
            foreach (var script in scripts) {
                var scriptLines = SqlHelpers.ParseSqlScript(script.Content);
                var transactionName = script.FileName;
                if (transactionName.Length > 31)
                    transactionName = transactionName.Substring(0, 31);
                if (script.IgnoreChangesButTrackFile)
                {
                    Logger.WriteLine("Ignoring: " + script.FileName + " (" + script.Id.ToString() + ")");                    
                }
                else
                {
                    Logger.WriteLine("Executing: " + script.FileName + " (" + script.Id.ToString() + ")");
                    SqlHelpers.ExecuteMultipleLineSqlTransaction(scriptLines, connection, transactionName);
                }
                SchemaHelpers.InsertScript(script);
                Logger.WriteLine("Success!");
            }
        }

        public void DontExecute() {
            foreach (var script in scripts) {
                Logger.WriteLine("I would have executed: " + script.FileName + " (" + script.Id.ToString() + ")");
            }
        }
    }
}
