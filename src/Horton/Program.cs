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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.ResetColor();
            }
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
                { "s|server=", "server hostname.\n(leave blank for \"localhost\"", v => options.ServerHostname = v },
                { "d|database=", "database name.", v => options.DatabaseName = v },
                { "u|username=", "username of the database connection.\n(leave blank for integrated security)", v => options.Username = v },
                { "p|password=", "password of the database connection.\n(required if username is provided)", v => options.Password = v },
                { "U|UNATTEND", "Surpress user acknowledgement before\nexecution.", v => options.Unattend = v != null },
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
