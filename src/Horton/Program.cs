using System;
using System.Collections.Generic;
using System.Linq;
using Horton.SqlServer;
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
            }
            finally
            {
                Console.ResetColor();
            }
        }

        public static void Print(string value)
        {
            Console.Write(value);
        }

        public static void PrintLine(string value)
        {
            Console.WriteLine(value);
        }

        public static void PrintLine()
        {
            Console.WriteLine();
        }

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

        public static void PrintErrorLine(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(value);
            Console.ForegroundColor = _originalConsoleConsole;
        }

        private static void MainInternal(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Horton. The simple database migration utility.");
            Console.WriteLine();

            var options = new HortonOptions();
            bool showHelp = false;

            var p = new OptionSet()
            {
                { "m|migrations=", "path to migration scripts.\n(leave blank for current directory)", v => options.MigrationsDirectoryPath = v },
                { "s|server=", "server hostname.\n(leave blank for \"localhost\")", v => options.ServerHostname = v },
                { "d|database=", "database name.\n(leave blank to look for \"database.name\")", v => options.DatabaseName = v },
                { "u|username=", "username of the database connection.\n(leave blank for integrated security)", v => options.Username = v },
                { "p|password=", "password of the database connection.\n(required if username is provided)", v => options.Password = v },
                { "U|UNATTEND", "Surpress user acknowledgement during\nexecution.", v => options.Unattend = v != null },
                //{ "v", "increase debug message verbosity", v => { if (v != null) ++verbosity; } },
                { "h|help|?",  "show help message and exit.", v => showHelp = v != null },
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
                Console.Write("horton: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `horton.exe --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            options.Command = HortonCommands.TryParseCommand(extra.FirstOrDefault() ?? "");

            options.TryGetDatabaseNameFromFile();

            string firstValidationMessage = "";
            if (!options.AssertValid(out firstValidationMessage))
            {
                Console.WriteLine(firstValidationMessage);
                Console.WriteLine("Try `horton.exe --help' for more information.");
                return;
            }

            Console.WriteLine();
            options.Command.Execute(options);
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: horton.exe [OPTIONS] [COMMAND]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine(" UPDATE\t\tExecutes current migrations if no conflicts exist.");
            Console.WriteLine(" INFO\t\tPrints the migrations that will execute on UPDATE.\n\t\tPrints any conflicting scripts.");
            Console.WriteLine(" SYNC\t\tResolves migration conflicts by updated checksums\n\t\tin database schema_info table.");
            Console.WriteLine(" HISTORY\tPrints all previously executed migrations.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine(" horton.exe -m \"\\path\\to\\migrations\" -s LOCALHOST -d Northwind -U");
            Console.WriteLine(" horton.exe -m \"\\path\\to\\migrations\" -s LOCALHOST -d Northwind -u sa -p pa55w0rd");
        }
    }
}
