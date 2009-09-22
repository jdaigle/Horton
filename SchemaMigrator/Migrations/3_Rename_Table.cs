using System;
using Microsoft.SqlServer.Management.Smo;

public class Rename_Table : Cridion.SchemaMigrator.Migration
{

    public override void Up()
    {
        /* we can get table references to work with */
        Table accounts = Database.Tables["accounts"];

        /* rename the table */
        accounts.Rename("externalaccounts");
        accounts.Alter();
    }

    public override void Down()
    {
        Table accounts = Database.Tables["externalaccounts"];
        accounts.Rename("accounts");
        accounts.Alter();
    }

}