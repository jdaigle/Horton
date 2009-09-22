using System;
using Microsoft.SqlServer.Management.Smo;

public class Change_Stuff : Cridion.SchemaMigrator.Migration
{

    public override void Up()
    {
        /* we can get table references to work with */
        Table accounts = Database.Tables["accounts"];

        /* add a column */
        Column desc = new Column(accounts, "description");
        desc.DataType = DataType.VarChar(100);
        desc.Nullable = false;
        desc.Create();

        /* we can rename objects too */
        Column accountname = accounts.Columns["name"];
        accountname.Rename("accountname");
        accountname.Alter();
    }

    public override void Down()
    {
        Table accounts = Database.Tables["accounts"];            
        
        accounts.Columns["description"].Drop();

        Column name = accounts.Columns["accountname"];
        name.Rename("name");
        name.Alter();
    }

}

