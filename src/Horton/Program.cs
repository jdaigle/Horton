using System;
using System.Collections.Generic;
using NDesk.Options;

namespace Horton
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MainInternal(args);
            }
            catch (Exception ex)
            {
                PrintErrorLine(ex.Message);
                PrintErrorLine(ex.StackTrace);
#if DEBUG
                throw;
#else
                Environment.Exit(1);
#endif
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public static void Print(string value) => Console.Write(value);

        public static void PrintLine(string value) => Console.WriteLine(value);

        private static readonly ConsoleColor _originalConsoleConsole = Console.ForegroundColor;

        public static void Print(ConsoleColor color, string value)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = _originalConsoleConsole;
        }

        public static void PrintLine(ConsoleColor color, string value)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = _originalConsoleConsole;
        }

        public static void PrintSuccess(string value) => Print(ConsoleColor.Green, value);

        public static void PrintSuccessLine(string value) => PrintLine(ConsoleColor.Green, value);

        public static void PrintErrorLine(string value) => PrintLine(ConsoleColor.Red, value);

        private static void MainInternal(string[] args)
        {
            var options = new HortonOptions();
            bool showHelp = false;
            bool printVersion = false;

            var p = new OptionSet()
            {
                { "d|database=", "database name.\n(required)", a => options.DatabaseName = a },
                { "f|files=", "path to migration scripts.\n(leave blank for current directory)", a => options.FilesPath = a },
                { "s|server=", "server hostname.\n(leave blank for \"(local)\")", a => options.ServerHostname = a },
                { "u|username=", "username of the database connection.\n(leave blank for integrated security)", a => options.Username = a },
                { "p|password=", "password of the database connection.\n(required if username is provided)", a => options.Password = a },
                { "c|connectionString=", "ADO.NET connection string.\n(optional, overrides other parameters)", a => options.ConnectionString = a },
                { "U|unattend", "Surpress user acknowledgement during execution.", a => options.Unattend = a != null },
                { "n|dryrun", "Only show scripts that would run without actually running.", a => options.DryRun = a != null },
                { "rerun", "Warn when a migration has been modified and re-run script.", a => options.WarnAndRerunModifiedMigrations = a != null },
                { "baseline", "Record scripts as a baseline without running.", a => options.RunBaseline = a != null },
                { "v|version",  "Print version number and exit.", a => printVersion = a != null },
                { "h|help|?",  "show help message and exit.", a => showHelp = a != null },
            };

            if (args.Length == 0)
            {
                showHelp = true;
            }

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                PrintErrorLine(e.Message);
                PrintLine("Try `horton.exe --help' for more information.");
                return;
            }

            if (!options.Unattend)
            {
                PrintLine("Horton. The simple database migration utility.");
            }

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            if (printVersion)
            {
                Console.WriteLine("Version: " + typeof(Program).Assembly.GetName().Version.ToString());
                return;
            }

            if (!options.AssertValid(out string firstValidationMessage))
            {
                PrintErrorLine(firstValidationMessage);
                PrintLine("Try `horton.exe --help' for more information.");
                Environment.Exit(1);
            }

            new MigrationRunner().Execute(options);
        }

        static void ShowHelp(OptionSet p)
        {
            PrintLine("Usage: horton.exe -d DATABASE [OPTIONS]");
            PrintLine(string.Empty);
            PrintLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            PrintLine(string.Empty);
            PrintLine("Examples:");
            PrintLine(" horton.exe -d Northwind -f \"\\path\\to\\migrations\"");
            PrintLine(" horton.exe -d Northwind -f \"\\path\\to\\migrations\" -s SERVER -u sa -p pa55w0rd -U");
        }
    }
}
