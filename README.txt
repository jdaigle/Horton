Simple Sql Patching Utility

--------------
About
--------------

Author: joseph@cridion.com
Version: 1.1
Website: http://github.com/jdaigle/SqlPatch

This is small utility provides the ability to apply T-SQL based patches or
migrations to an MS SQL database. A patch is a forward only migration of a 
SQL database schema.

--------------
Change History
--------------

Version 1.1 (Feb 1, 2010)
- Added a primary key to the schema_info table upon initial creation (does not upgrade existing table)
- Added support for SQL View and Stored Procedure generation (DROP/CREATE)

Version 1.0 (Oct 4, 2009)
- Initial Release

--------------
How to use
--------------

The executable accepts the following command line arguments:

/m  PATH			Migration Directory Path
/vw  PATH			Views Directory Path
/sp  PATH			Stored Procedures Directory Path
/s  SERVER		SQL Server Network Address
/d  DATABASE		SQL Server Database Name
/i 	     			Integrated SQL Server Security
/u  USERNAME		SQL Server Login Username
/p  PASSWORD		SQL Server Login Password

When you use the "/i" argument you do not need to specify the username or
password.

Examples:

SqlPatch.exe /m \Migrations /s .\SQLEXPRESS /d Northwind /i
SqlPatch.exe /m \Migrations /sp \sprocs /vw \views /s .\SQLEXPRESS /d Northwind /i
SqlPatch.exe /m \Migrations /s .\SQLEXPRESS /d Northwind /u sa /p pa55w0rd
SqlPatch.exe /m "c:\Example Folder\Migrations" /s .\SQLEXPRESS /d Northwind /i

Migration Change Scripts:

Each T-SQL patch can represent a single database change. These are executed in a
specific order determined by the filename convention:

"0001_Description.sql"

Basically the program accepts any ".sql" file which begins with text that can
be parsed into an integer followed by an undercore and descriptive file. For
example:

0001_Initial_Create_Tables.sql
002_Add_Some_Columns.sql
00003_Drop_Some_Columns.sql
4_Add_New_Table.sql

These are executed in order based on the integer prefix and not the alphabet.

The T-SQL should generally follow these rules:

1. Should be single change, or a set of related changes.
2. Should protect data, do no drop existing data without a known backup.
3. The script should be able to run against a production system without breaking.
4. (Optional) the script should be able to run multiple times with side-effects

The utility will create a table in the target database if it doesn't exist which
will contain information about what migrations/patches have already been run
against the database. The table has the following schema:

CREATE TABLE schema_info ( 
	version int NOT NULL, 
	migration_script varchar(255) NOT NULL, 
	CONSTRAINT [PK_schema_info] PRIMARY KEY CLUSTERED ( [version])
	)
	
The first column contains the integer of the migration/patch that was run, and the
second column contains the file-name of the migration/patch that was run.

When the utility executes against the database, it queries for the last (version with
greatest integer value) migration/patch run. It will then run any migration/patches
found in the migration directory which are greater than that value. As each
script executes it is stored in the schema_info table.

Each migration/patch is executed within a transaction. If for any reason it fails,
the transaction is rolled-back and the process is aborted.
