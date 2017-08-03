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

        public void Initialize()
        {
            AssertNotDisposed();

            AppliedMigrations.Clear();

            using (var cmd = Connection.CreateCommand())
            {
                using (var tran = Connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = CreateSchemaInfoTable;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = UpgradeV4toV5;
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                }
                cmd.Transaction = null;

                cmd.CommandText = "SELECT FileName, ContentHash, [Type], AppliedUTC FROM dbo.schema_info ORDER BY AppliedUTC;";
                using (var reader = cmd.ExecuteReader())
                {
                    var fileNameOrdinal = reader.GetOrdinal(nameof(AppliedMigrationRecord.FileName));
                    var contentHashOrdinal = reader.GetOrdinal(nameof(AppliedMigrationRecord.ContentHash));
                    var typeOrdinal = reader.GetOrdinal(nameof(AppliedMigrationRecord.Type));
                    var appliedUTCOrdinal = reader.GetOrdinal(nameof(AppliedMigrationRecord.AppliedUTC));
                    while (reader.Read())
                    {
                        var record = new AppliedMigrationRecord()
                        {
                            FileName = reader.GetString(fileNameOrdinal),
                            ContentHash = reader.GetString(contentHashOrdinal),
                            Type = reader.GetByte(typeOrdinal),
                            AppliedUTC = reader.GetDateTime(appliedUTCOrdinal),
                        };
                        AppliedMigrations.Add(record);
                    }
                }
            }
        }

        public List<AppliedMigrationRecord> AppliedMigrations { get; } = new List<AppliedMigrationRecord>();

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
                RecordMigration(transaction, migration, sw.Elapsed.TotalMilliseconds);
                transaction.Commit();
            }
        }

        public static string[] ParseSqlScript(string script) => CommandSeperatorRegex.Split(script);

        private void RecordMigration(SqlTransaction transaction, ScriptFile migration, double transactionDuractionMS)
        {
            AssertNotDisposed();
            using (var cmd = Connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = InsertInfo;

                var param_FileName = cmd.Parameters.Add("@FileName", SqlDbType.VarChar, 255);
                var param_ContentHash = cmd.Parameters.Add("@ContentHash", SqlDbType.VarChar, 40);
                var param_Content = cmd.Parameters.Add("@Content", SqlDbType.VarChar, -1);
                var param_Type = cmd.Parameters.Add("@Type", SqlDbType.TinyInt);
                var param_TransactionDuractionMS = cmd.Parameters.Add("@TransactionDuractionMS", SqlDbType.Float);

                param_FileName.Value = migration.FileName;
                param_ContentHash.Value = migration.ContentHash;
                param_Content.Value = migration.Content;
                param_Type.Value = migration.TypeCode;
                param_TransactionDuractionMS.Value = transactionDuractionMS;

                cmd.ExecuteNonQuery();
            }
        }

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

        private const string CreateSchemaInfoTable = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.schema_info') AND type in (N'U'))
CREATE TABLE [dbo].[schema_info] (
    [Id] [int] NOT NULL IDENTITY(1,1),
    [FileName] VARCHAR(255) NOT NULL,
    [ContentHash] VARCHAR(40) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [Type] TINYINT NOT NULL,
    [AppliedUTC] DATETIME2 NOT NULL,
    [TransactionDuractionMS] float NOT NULL,
    [SystemUser] VARCHAR(100) NOT NULL CONSTRAINT DF_schemainfo_SystemUser DEFAULT (SYSTEM_USER),
    CONSTRAINT [PK_schema_info] PRIMARY KEY CLUSTERED ([Id]),
);
";

        private const string UpgradeV4toV5 = @"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_FileNameMD5Hash' AND object_id = OBJECT_ID('dbo.schema_info'))
    DROP INDEX UX_FileNameMD5Hash ON [dbo].[schema_info];

IF EXISTS (SELECT * FROM sys.columns WHERE name = 'FileNameMD5Hash' AND object_id = OBJECT_ID('dbo.schema_info'))
    EXEC dbo.sp_executesql @statement = N'ALTER TABLE dbo.schema_info DROP COLUMN [FileNameMD5Hash];';

IF EXISTS (SELECT * FROM sys.columns WHERE name = 'ContentSHA1Hash' AND object_id = OBJECT_ID('dbo.schema_info'))
    EXEC sp_rename 'dbo.schema_info.ContentSHA1Hash', 'ContentHash', 'COLUMN';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Content' AND object_id = OBJECT_ID('dbo.schema_info'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'ALTER TABLE dbo.schema_info ADD [Content] NVARCHAR(MAX) NULL;';
    EXEC dbo.sp_executesql @statement = N'UPDATE dbo.schema_info SET [Content] = '';''';
    EXEC dbo.sp_executesql @statement = N'ALTER TABLE dbo.schema_info ALTER COLUMN [Content] NVARCHAR(MAX) NOT NULL;';
END
";

        private const string InsertInfo = @"
INSERT INTO [dbo].[schema_info]
           ([FileName]
           ,[ContentHash]
           ,[Content]
           ,[Type]
           ,[AppliedUTC]
           ,[TransactionDuractionMS]
           ,[SystemUser])
     VALUES
           (@FileName
           ,@ContentHash
           ,@Content
           ,@Type
           ,GETUTCDATE()
           ,@TransactionDuractionMS
           ,SYSTEM_USER);
";
    }
}
