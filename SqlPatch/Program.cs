using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace SqlPatch
{
    public class Program
    {

        public static void Main(string[] args)
        {
            try
            {
                if (!ParseCommandLineArgs(args))
                {
                    OutputHelpMessage();
                    return;
                }
                Logger.WriteLine("Simple Sql Patcher version 2\n");

                Logger.WriteLine("Loading script files...");
                Logger.Indent();
                var fileLoader = new FileLoader(Configuration.World.ScriptsDirectoryPath);
                fileLoader.LoadAllFiles();
                Logger.Unindent();
                Logger.WriteLine(fileLoader.Files.Count + " scripts loaded.\n");

                Logger.WriteLine("Checking database schema...");
                Logger.Indent();
                SchemaHelpers.EnsureSchemaInfoTable();
                Logger.Unindent();
                Logger.WriteLine("Database schema check complete.\n");

                Logger.WriteLine("Checking for applied scripts...");
                Logger.Indent();
                var changedDatabaseObjects = new List<ScriptFile>();
                var appliedScripts = SchemaHelpers.GetScripts();
                // This loop contains horrors that cannot be unseen, one day I'll be brave enough to refactor it
                foreach (var script in appliedScripts)
                {
                    if (fileLoader.Files.ContainsKey(script.Id))
                    {
                        var file = fileLoader.Files[script.Id];
                        if (!file.Matches(script))
                        {
                            if (file.Type == ScriptType.ChangeScript)
                            {
                                Logger.WriteLine(string.Format("WARNING! The script {0} has changed since it was applied on {1}.", file.FileName, script.Applied.Date.ToShortDateString()));
                                if (!Configuration.World.Unattended)
                                {
                                    Console.WriteLine("\nType RERUN to run the script again type, IGNORE to mark the file as executed, or SKIP to skip this file: ");
                                    var option = Console.ReadLine();
                                    if (option == "RERUN")
                                        changedDatabaseObjects.Add(file);
                                    else if (option == "IGNORE")
                                    {
                                        file.IgnoreChangesButTrackFile = true;
                                        changedDatabaseObjects.Add(file);
                                    }
                                    else if (option != "SKIP")
                                        Abort("User aborted due to a script file changing that was already applied.");
                                    Console.WriteLine();
                                }
                            }
                            else
                            {
                                changedDatabaseObjects.Add(file);
                            }
                        }
                    }
                }
                Logger.Unindent();
                Logger.WriteLine("Database is ready to be patched!\n");

                var applied = new HashSet<Guid>(appliedScripts.Select(x => x.Id));
                var scripts = new List<ScriptFile>();
                foreach (var script in fileLoader.Files)
                {
                    if (!applied.Contains(script.Key))
                        scripts.Add(script.Value);
                }
                scripts.AddRange(changedDatabaseObjects);
                Logger.WriteLine("Ready to execute " + scripts.Count + " new scripts...");
                Logger.Indent();
                var engine = new ScriptEngine(scripts, SchemaHelpers.CreateConnectionString());
                if (Configuration.World.Apply)
                    engine.Execute();
                else if (!Configuration.World.Apply)
                    engine.DontExecute();
                Logger.Unindent();
                Logger.WriteLine("Process Complete.");
            }
            catch (Exception e)
            {
                Logger.WriteLine("THERE WAS AN ERROR!");
                Logger.WriteLine(string.Empty);
                Logger.WriteLine(e.Message);
                Logger.WriteLine(string.Empty);
                Logger.WriteLine(e.StackTrace);
                var innerExcepton = e.InnerException;
                while (innerExcepton != null)
                {
                    Logger.WriteLine(string.Empty);
                    Logger.WriteLine(innerExcepton.Message);
                    Logger.WriteLine(string.Empty);
                    Logger.WriteLine(innerExcepton.StackTrace);
                    innerExcepton = innerExcepton.InnerException;
                }
                Environment.Exit(-1);
            }
        }

        public static void Abort(string reason)
        {
            Logger.Reset();
            Logger.WriteLine("Process aborted: " + reason);
            Environment.Exit(1);
        }

        private static void OutputHelpMessage()
        {
            var output = new StringBuilder();
            output.AppendLine();
            output.AppendLine("Simple Sql Patcher Command Line Arguments");
            output.AppendLine("---");
            output.AppendLine("-m  PATH\t\tDatabase Scripts Directory (no spaces or surrounded by quotes)");
            output.AppendLine("-s  SERVER\tSQL Server Network Address");
            output.AppendLine("-d  DATABASE\tSQL Server Database Name");
            output.AppendLine("-i  \t\tIntegrated SQL Server Security (instead of -u and -p)");
            output.AppendLine("-u  USERNAME\tSQL Server Login Username (requires -p)");
            output.AppendLine("-p  PASSWORD\tSQL Server Login Password");
            output.AppendLine("-a  \t\tUnattended process (useful for integration environments)");
            output.AppendLine("-f  \t\tActually runs the script.");
            output.AppendLine();
            output.AppendLine("EXAMPLES:");
            output.AppendLine(@"SqlPatch.exe -m Scripts -s .\SQLEXPRESS -d Northwind -i");
            output.AppendLine(@"SqlPatch.exe -m Scripts -s .\SQLEXPRESS -d Northwind -u sa -p pa55w0rd");
            output.AppendLine("SqlPatch.exe -m \"c:\\Example Folder\\Scripts\" -s .\\SQLEXPRESS -d Northwind -i");
            output.AppendLine();
            Console.Out.WriteLine(output.ToString());
        }

        private static bool ParseCommandLineArgs(string[] args)
        {
            if (args.Length < 7)
                return false;
            for (int i = 0; i < args.Length; i++)
            {
                bool lastArg = i == args.Length - 1;
                var argument = args[i];
                if (argument.Equals("-m", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.ScriptsDirectoryPath = args[i + 1];
                }
                else if (argument.Equals("-s", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Server = args[i + 1];
                }
                else if (argument.Equals("-d", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Database = args[i + 1];
                }
                else if (argument.Equals("-i", StringComparison.OrdinalIgnoreCase))
                {
                    Configuration.World.IntegratedSecurity = true;
                }
                else if (argument.Equals("-a", StringComparison.OrdinalIgnoreCase))
                {
                    Configuration.World.Unattended = true;
                }
                else if (argument.Equals("-u", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Username = args[i + 1];
                }
                else if (argument.Equals("-p", StringComparison.OrdinalIgnoreCase))
                {
                    if (lastArg)
                        return false;
                    Configuration.World.Password = args[i + 1];
                }
                else if (argument.Equals("-f", StringComparison.OrdinalIgnoreCase))
                {
                    Configuration.World.Apply = true;
                }
            }
            return Configuration.World.IsValid;
        }

    }
}
