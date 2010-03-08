Simple Sql Patching Utility

--------------
About
--------------

Author: joseph@cridion.com
Version: 2.0
Website: http://github.com/jdaigle/SqlPatch

This is small utility provides the ability to apply T-SQL based patches or
migrations to an MS SQL database. A patch is a forward only migration of a 
SQL database schema.

--------------
Change History
--------------

Version 2.0 (March 8, 2010)
 - Complete rewrite... major breaking changes.

Version 1.1 (Feb 1, 2010)
- Added a primary key to the schema_info table upon initial creation (does not upgrade existing table)
- Added support for SQL View and Stored Procedure generation (DROP/CREATE)

Version 1.0 (Oct 4, 2009)
- Initial Release

--------------
How to use
--------------

The executable accepts the following command line arguments:

-m  PATH			Migration Directory Path
-s  SERVER		    SQL Server Network Address
-d  DATABASE		SQL Server Database Name
-i 	     			Integrated SQL Server Security
-u  USERNAME		SQL Server Login Username
-p  PASSWORD		SQL Server Login Password
-a                  Unattended process (useful for integration environments)

When you use the "-i" argument you do not need to specify the username or
password.

Examples:

SqlPatch.exe -m Scripts -s .\SQLEXPRESS -d Northwind -i
SqlPatch.exe -m Scripts -s .\SQLEXPRESS -d Northwind -u sa -p pa55w0rd
SqlPatch.exe -m "c:\Example Folder\Scripts" -s .\SQLEXPRESS -d Northwind -i

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
will contain information about what patches have already been run
against the database.

When the utility executes against the database it queries the schema_info stored in
database. It will first warn about any change scripts that have change since it was
run against the database. It will then run any scripts that haven't been run, logging
as it goes.

You can optionally create directories under your scripts folder called "views" and
"sprocs". The tool will detect these directories as special database object directories.
Any scripts in these directories will be run by the tool anytime a change is detected 
(and of course if the script has never been run against the database).

Each script is executed within a transaction. If for any reason it fails,
the transaction is rolled-back and the process is aborted.
