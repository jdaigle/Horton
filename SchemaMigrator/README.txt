----
Contact and Copyright Notice
----

All binaries are Copyright 2007 Cridion Technologies.

You can contact Cridion at webmaster@cridion.com.

----
32 bit versus 64 bit Version Note!!!
----

When using SMO Migrations on a machine with the 64bit version of SQL Server, you need
to insure that the assemblies included in this Migrator.exe directory are supported
for 64bit SQL. You also must install the Microsoft SQL Server 2005 Management Objects Collection
for 64bit SQL Server. Currently available at:

http://www.microsoft.com/downloads/details.aspx?FamilyID=d09c1d60-a13c-4479-9b91-9e8b9d835cdc&displaylang=en

If you are using 32bit SQL, or only uses SQL Script based Migrations, the above does not apply.

----
How to use SQL .NET Migrations
----

SQL .NET Migrations is a database schema management tool modeled
identical to Rails Migrations. Schema versions are coded into source files.
These source files are sequentically numbered indicating the version number it
represents.

There are two types of migrations. The first is an SMO Migration, that uses the
the SQL Mamanagment Objects API. The second is based on SQL Script pairs.

----
SMO Migrations
----

Source files are compiled at runtime. Each file contains one class that extends 
from Cridion.SchemaMigrator.Migration, which is an abstract class. It must override
the methods Up() and Down().

The method Up() defines the operations to bring the database up to this specific
schema version given the previous state. The Down() method defines operations to reverse
Up() rolling the database schema back to it's previous schema state.

It is heavily suggested that all operations use the Microsoft.SqlServer.Management.Smo
namespace for database object management. This provides strong typing and compile time checking
of source errors. Runtime will produce meaningful exceptions.

The class can be named anything, in any none-global namespace (to avoid naming conflicts). Each class
is loaded independently into memory, so they will not conflict.

Filename format is as follows: 001_classname.cs

----
SQL Migrations
----

These are pairs of Transactional SQL Scripts. The important aspect is the filename. It must begin with
the schema number, followed by an underscore, followed by either "up" or "down" followed by an underscore,
followed by a name, followed by ".sql".

Example: 001_up_name.sql
		 001_down_name.sql

Scripts must come as pairs.

The scripts are composed of Transactional SQL commands. Each command must be delimited by the statement GO.

----
Other Stuff
----

The schema version is dependant on file names. They must be consecutive, starting at 1.
The number of significant digits is not important, as long as it represents an integer value.
The version number must be the first part of the filename, followed by the underscore character.

Simple run the program. You can pass in a command line argument indicated the schema version to roll to.