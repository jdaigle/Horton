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
    public class SqlHelpers
    {

        public static String[] ParseSqlScript(String ScriptText)
        {
            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.Split(ScriptText);
        }

        public static SqlConnection GetConnection(String ConnectionString)
        {
            return new SqlConnection(ConnectionString);
        }

        public static int ExecuteMultipleLineSqlTransaction(String[] lines, SqlConnection connection, String TransactionName)
        {
            int returnvalue = 0;
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction(TransactionName);
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.Connection = connection;
                cmd.Transaction = transaction;

                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        cmd.CommandText = line;
                        cmd.CommandType = CommandType.Text;

                        try
                        {
                            returnvalue = cmd.ExecuteNonQuery();
                        }
                        catch (SqlException e)
                        {
                            transaction.Rollback();
                            connection.Close();
                            throw e;
                        }
                    }
                }
            }

            transaction.Commit();
            connection.Close();
            return returnvalue;
        }


    }
}
