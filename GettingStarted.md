# Getting Started

Execute `horton.exe --help` to see the command syntax

Forward-only database migrations help achieve consistent database upgrades and schema versioning. Instead of comparing two database schemas and generating a diff script, we explicitly design the change scripts first. This ensures that changes are applied in a predictable and correct way. It also allows us to migrate data which is often a critical requirement, and helps achieve multi-step database schema refactorings such as renaming or combining/splitting columns.

**Horton is designed to work with SQL migration scripts.** In your version control system, you should create a directory per database for your migration scripts.

Check out the `\samples\` directory in this repository for examples of how I would set it up.

## Command Syntax

    Usage: horton.exe [OPTIONS] [COMMAND]
    
    Commands:
     UPDATE         Executes current migrations if no conflicts exist.
     INFO           Prints the migrations that will execute on UPDATE.
                    Prints any conflicting scripts.
     SYNC           Resolves migration conflicts by updated checksums
                    in database schema_info table.
     HISTORY        Prints all previously executed migrations.
    
    Options:
      -m, --migrations=VALUE     path to migration scripts.
                                   (leave blank for current directory)
      -s, --server=VALUE         server hostname.
                                   (leave blank for "localhost")
      -d, --database=VALUE       database name.
                                   (leave blank to look for "database.name")
      -u, --username=VALUE       username of the database connection.
                                   (leave blank for integrated security)
      -p, --password=VALUE       password of the database connection.
                                   (required if username is provided)
      -c, --connectionString=VALUE
                                 ADO.NET connection string.
                                   (optional, overrides other parameters)
      -U, --UNATTEND             Surpress user acknowledgement during
                                   execution.
      -v, --version              Print version number and exit.
      -h, --help, -?             show help message and exit.
    
    Examples:
     horton.exe -m "\path\to\migrations" -s LOCALHOST -d Northwind -U
     horton.exe -m "\path\to\migrations" -s LOCALHOST -d Northwind -u sa -p pa55w0rd

### `database.name`

In the root of your migration script directory, you can create a file name `database.name`. Horton will detect and read this file. Whatever is on the first line of the file will be used as the `database name` option. But if you specify the `database name` command line option, the file is ignored.

## File Names

The file name of a script is important. The name MUST begin with some text that can be parsed as integer. The number MUST be followed by an underscore and then some text. For example:

    001_extend_emailaddress_to_255_chars.sql

Notes:

- The integer may be left-padded with zeros. This is useful if you know that you're likely to have hundreds or thousands of scripts and you want them to appear consistent in your file browser.
- The integer value is used to order the scripts for execution. You MAY have gaps in the numbering. You MAY have multiple scripts with the same number (they will execute in a non-deterministic order). You MAY add a script with a number lower than what was previously executed. This makes it easier to work with branches, or where multiple developers are committing migration scripts.

**Once you name a file, you should not rename it.** Horton will hash the file name and use that as a key to determine whether or not the migration was executed against a particular database instance.

## Scripting Migrations

**Each migration script is executed in a separate database transaction.** That means each script will either succeed or fail completely. Horton will stop executing scripts on the first error.

**You may separate commands in the migration script using the `GO` keyword on it's own line.** This is very similar to how SQL Server Management Studio will execute a script. Essentially Horton will split the file on those lines and execute each part as a separate command within that transaction. Try to do this only when it's necessary.

Some DDL statements, such as `CREATE VIEW` are required to be the only statement executed in a command.

Because you don't always know when a particular script will execute **it's important that scripts be safe and/or idempotent.** For example, before add/dropping a column you should check to see whether or not it exists. **Err on the side of caution and test for preconditions before executing a change.**

**DO NOT MODIFY SCRIPTS!** Especially after you've committed, and super-especially if the script was ever executed. Horton *will* detect and *warn* when a script has been modified.

If a script was added and executed by mistake, you should delete the bad script and create a new script which safely corrects the mistake.

### `schema_info`

Horton will automatically create and maintain a table named `schema_info` in the target database. This table contains a history of each migration executed. Horton uses this history to determine which scripts have been executed, and which have not. Additionally it contains a hash of the file's contents, and so it will *warn* when the file contents have since been modified.

You can query this table directory from Horton by executing `horton.exe` with the `HISTORY` command.

## Supported Databases

At the moment, Horton only supports Microsoft SQL Server. However it's built to use ADO.NET providers. So any database that has an ADO.NET provider could, in theory, be supported. If you want to extend Horton to some other ADO.NET provider, then let's talk!