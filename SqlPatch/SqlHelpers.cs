using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SqlPatch {
    public class SqlHelpers {
        public static string[] ParseSqlScript(string script) {
            var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.Split(script);
        }

        public static void ExecuteMultipleLineSqlTransaction(string[] lines, SqlConnection connection, string transactionName) {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            var transaction = connection.BeginTransaction(transactionName);
            try {
                foreach (string line in lines) {
                    try {
                        if (line.Length == 0)
                            continue;
                        using (SqlCommand cmd = connection.CreateCommand()) {
                            cmd.Connection = connection;
                            cmd.Transaction = transaction;
                            cmd.CommandText = line;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    } catch (SqlException e) {
                        throw new ScriptExecutionException(line, "An error occured executing a script.", e);
                    }
                }
                transaction.Commit();
            } catch (Exception) {
                transaction.Rollback();
                throw;
            } finally {
                connection.Close();
            }
        }
    }
}
