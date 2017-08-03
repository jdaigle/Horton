using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Horton.Scripts;

namespace Horton.SqlServer
{
    public class SqlServerDatabase : IDisposable
    {
        public static readonly Regex CommandSeperatorRegex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);


        public SqlServerDatabase(HortonOptions options)
        {
            Connection = new SqlConnection(options.CreateConnectionString());
            Connection.Open();
        }

        public int CommandTimeout { get; } = 60;

        public SqlConnection Connection { get; }

        public void Initialize() => throw new NotImplementedException();

        public IList<AppliedMigrationRecord> AppliedMigrations { get; internal set; }

        public void Run(ScriptFile migration, bool baseline)
        {
            AssertNotDisposed();
            var commands = ParseSqlScript(migration.Content);
            using (var transaction = Connection.BeginTransaction())
            {
                var sw = Stopwatch.StartNew();
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandTimeout = CommandTimeout;
                    foreach (var commandText in commands)
                    {
                        if (string.IsNullOrWhiteSpace(commandText))
                        {
                            continue;
                        }
                        cmd.CommandText = commandText;
                        if (!baseline)
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                sw.Stop();
                //RecordMigration(transaction, migration, sw.Elapsed.TotalMilliseconds);
                transaction.Commit();
            }
        }

        public static string[] ParseSqlScript(string script) => CommandSeperatorRegex.Split(script);

        private bool disposed;

        private void AssertNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(SqlServerDatabase));
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                try
                {
                    if (Connection.State == ConnectionState.Open)
                    {
                        Connection.Close();
                    }
                    Connection.Dispose();
                }
                finally
                {
                    disposed = true;
                }
            }
        }
    }
}
