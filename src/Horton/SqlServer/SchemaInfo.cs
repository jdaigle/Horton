//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.Text.RegularExpressions;

//namespace Horton.SqlServer
//{
//    public class SchemaInfo : IDisposable
//    {
//        private readonly List<AppliedMigrationRecord> appliedMigrations = new List<AppliedMigrationRecord>();

//        public SchemaInfo(HortonOptions options)
//        {
//            Connection = new SqlConnection(options.CreateConnectionString());
//            Connection.Open();
//        }

//        public DbConnection Connection { get; }

//        public void InitializeTable()
//        {
//            AssertNotDisposed();
//            appliedMigrations.Clear();

//            using (var cmd = Connection.CreateCommand())
//            {
//                cmd.CommandText = CreateSchemaInfoTable;
//                cmd.ExecuteNonQuery();

//                cmd.CommandText = SelectExistingSchema;
//                using (var reader = cmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        var record = new AppliedMigrationRecord();
//                        record.FileName = reader.GetString(0);
//                        record.FileNameMD5Hash = reader.GetGuid(1);
//                        record.ContentSHA1Hash = reader.GetString(2);
//                        record.Type = reader.GetByte(3);
//                        record.AppliedUTC = reader.GetDateTime(4);
//                        record.SystemUser = reader.GetString(5);
//                        appliedMigrations.Add(record);
//                    }
//                }
//                appliedMigrations.Sort((a, b) => a.AppliedUTC.CompareTo(b.AppliedUTC));
//            }
//        }

//        public IReadOnlyList<AppliedMigrationRecord> AppliedMigrations { get { return appliedMigrations; } }

//        public void ApplyMigration(ScriptFile migration)
//        {
//            AssertNotDisposed();
//            var commands = ParseSqlScript(migration.Content);
//            using (var transaction = Connection.BeginTransaction() as SqlTransaction)
//            {
//                var sw = new Stopwatch();
//                sw.Restart();
//                foreach (var command in commands)
//                {
//                    if (string.IsNullOrWhiteSpace(command))
//                        continue;
//                    using (SqlCommand cmd = Connection.CreateCommand() as SqlCommand)
//                    {
//                        cmd.Transaction = transaction;
//                        cmd.CommandTimeout = 3000;
//                        cmd.CommandText = command;
//                        var rowsAffected = cmd.ExecuteNonQuery();
//                    }
//                }
//                sw.Stop();
//                if (migration is RepeatableScript)
//                {
//                    // delete any existing record before inserting
//                    TryDeleteMigration(transaction, migration);
//                }
//                RecordMigration(transaction, migration, sw.Elapsed.TotalMilliseconds);
//                transaction.Commit();
//            }
//        }

//        public static string[] ParseSqlScript(string script)
//        {
//            var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
//            return regex.Split(script);
//        }

//        public void ResyncMigration(ScriptFile migration)
//        {
//            AssertNotDisposed();
//            using (var transaction = Connection.BeginTransaction() as SqlTransaction)
//            {
//                TryDeleteMigration(transaction, migration);
//                RecordMigration(transaction, migration, 0);
//                transaction.Commit();
//            }
//        }

//        private void TryDeleteMigration(DbTransaction transaction, ScriptFile migration)
//        {
//            using (SqlCommand cmd = Connection.CreateCommand() as SqlCommand)
//            {
//                cmd.Transaction = transaction as SqlTransaction;
//                cmd.CommandText = DeleteByFileNameMD5Hash;

//                var param_FileNameMD5Hash = cmd.Parameters.Add("@FileNameMD5Hash", SqlDbType.UniqueIdentifier);
//                param_FileNameMD5Hash.Value = migration.FileNameHash;

//                cmd.ExecuteNonQuery();
//            }
//        }

//        private void RecordMigration(DbTransaction transaction, ScriptFile migration, double transactionDuractionMS)
//        {
//            AssertNotDisposed();
//            using (SqlCommand cmd = Connection.CreateCommand() as SqlCommand)
//            {
//                cmd.Transaction = transaction as SqlTransaction;
//                cmd.CommandText = InsertInfo;

//                var param_FileName = cmd.Parameters.Add("@FileName", SqlDbType.VarChar, 255);
//                var param_FileNameMD5Hash = cmd.Parameters.Add("@FileNameMD5Hash", SqlDbType.UniqueIdentifier);
//                var param_ContentSHA1Hash = cmd.Parameters.Add("@ContentSHA1Hash", SqlDbType.VarChar, 40);
//                var param_Type = cmd.Parameters.Add("@Type", SqlDbType.TinyInt);
//                var param_TransactionDuractionMS = cmd.Parameters.Add("@TransactionDuractionMS", SqlDbType.Float);

//                param_FileName.Value = migration.FileName;
//                param_FileNameMD5Hash.Value = migration.FileNameHash;
//                param_ContentSHA1Hash.Value = migration.ContentSHA1Hash;
//                param_Type.Value = migration.TypeCode;
//                param_TransactionDuractionMS.Value = transactionDuractionMS;

//                cmd.ExecuteNonQuery();
//            }
//        }

//        private bool disposed;

//        private void AssertNotDisposed()
//        {
//            if (disposed)
//                throw new ObjectDisposedException(nameof(SchemaInfo));
//        }

//        public void Dispose()
//        {
//            if (!disposed)
//            {
//                try
//                {
//                    if (Connection.State == ConnectionState.Open)
//                        Connection.Close();
//                    Connection.Dispose();
//                }
//                finally
//                {
//                    disposed = true;
//                }
//            }
//        }

//        private const string CreateSchemaInfoTable = @"
//IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.schema_info') AND type in (N'U'))
//CREATE TABLE [dbo].[schema_info] (
//    [Id] [int] NOT NULL IDENTITY(1,1),
//    [FileName] VARCHAR(255) NOT NULL,
//    [FileNameMD5Hash] UNIQUEIDENTIFIER NOT NULL,
//    [ContentSHA1Hash] VARCHAR(40) NOT NULL,
//    [Type] TINYINT NOT NULL,
//    [AppliedUTC] DATETIME2 NOT NULL,
//    [TransactionDuractionMS] float NOT NULL,
//    [SystemUser] VARCHAR(100) NOT NULL CONSTRAINT DF_schemainfo_SystemUser DEFAULT (SYSTEM_USER),
//    CONSTRAINT [PK_schema_info] PRIMARY KEY CLUSTERED ([Id]),
//);

//IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_FileNameMD5Hash' AND object_id = OBJECT_ID('dbo.schema_info'))
//    CREATE UNIQUE NONCLUSTERED INDEX UX_FileNameMD5Hash ON [dbo].[schema_info] ([FileNameMD5Hash]);
//";

//        private const string SelectExistingSchema = @"
//SELECT
//    FileName        -- 0
//   ,FileNameMD5Hash -- 1
//   ,ContentSHA1Hash -- 2
//   ,[Type]          -- 3
//   ,AppliedUTC      -- 4
//   ,SystemUser      -- 5
//FROM dbo.schema_info;
//";

//        private const string InsertInfo = @"
//INSERT INTO [dbo].[schema_info]
//           ([FileName]
//           ,[FileNameMD5Hash]
//           ,[ContentSHA1Hash]
//           ,[Type]
//           ,[AppliedUTC]
//           ,[TransactionDuractionMS]
//           ,[SystemUser])
//     VALUES
//           (@FileName
//           ,@FileNameMD5Hash
//           ,@ContentSHA1Hash
//           ,@Type
//           ,GETUTCDATE()
//           ,@TransactionDuractionMS
//           ,SYSTEM_USER);
//";

//        private const string DeleteByFileNameMD5Hash = @"
//DELETE FROM [dbo].[schema_info]
//WHERE FileNameMD5Hash = @FileNameMD5Hash;
//";
//    }
//}
