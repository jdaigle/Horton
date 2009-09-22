using System;
using Microsoft.SqlServer.Management.Smo;

public class Create_Tables : Cridion.SchemaMigrator.Migration
{

    public override void Up()
    {
        /* Accounts Table */
        Table accounts = new Table(Database, "accounts");
        
        /* need a column */
        Column ID = new Column(accounts, "id");
        ID.DataType = DataType.Int;
        ID.Identity = true;
        ID.IdentityIncrement = 1;
        ID.IdentitySeed = 0;
        ID.Nullable = false;

        /* a table cannot be created if it doesn't have at least 1 column */
        accounts.Columns.Add(ID);
        accounts.Create();

        /* now we can create child objects directly */

        /* our tables primary key */
        Index idx = new Index(accounts, "PK_accounts");            
        idx.IndexedColumns.Add(new IndexedColumn(idx, "id"));
        idx.IsClustered = true;
        idx.IsUnique = true;
        idx.IndexKeyType = IndexKeyType.DriPrimaryKey;
        idx.Create();

        /* another column */
        Column name = new Column(accounts, "name");
        name.DataType = DataType.VarChar(50);
        name.Nullable = false;
        name.Create();

        /* another column */
        Column email = new Column(accounts, "email");
        email.DataType = DataType.VarChar(100);
        email.Nullable = false;
        email.Create();



    }

    public override void Down()
    {
        /* we should ENSURE that running this will reverse what we created, regardless, just to be safe */
        if (Database.Tables.Contains("accounts"))
        {
            Table accounts = Database.Tables["accounts"];
            accounts.Drop();
        }
    }

}
