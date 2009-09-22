using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace Cridion.SchemaMigrator
{
    public abstract class Migration
    {
        private String _DatabaseConnectionString;
        public String DatabaseConnectionString
        {
            get { return _DatabaseConnectionString; }
        }

        private Server _Server;
        public Server Server
        {
            get { return _Server; }
        }

        private Database _Database;
        public Database Database
        {
            get { return _Database; }
        }        

        public Migration()
        {
            _DatabaseConnectionString = Configuration.CurrentConfig.ConnectionString;
            _Server = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(DatabaseConnectionString)));

            if (!_Server.Databases.Contains(_Server.ConnectionContext.DatabaseName))
                throw new Exception("Database: " + _Server.ConnectionContext.DatabaseName + " not found.");

            _Database = _Server.Databases[_Server.ConnectionContext.DatabaseName];
        }

        public abstract void Up();

        public abstract void Down();        

    }
}
