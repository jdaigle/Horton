using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Cridion.SchemaMigrator
{
    public class MigrationException : Exception
    {

        public SQLMigration SQLMigration { get; private set; }

        public String Command { get; private set; }

        public SqlException SqlException { get; private set; }

        public MigrationException(SQLMigration Migration, String Command, String Message, SqlException SqlException) : base(Message)
        {
            this.SQLMigration = Migration;
            this.Command = Command;
            this.SqlException = SqlException;
        }

    }
}
