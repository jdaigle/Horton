using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDeploy
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
                Logger.WriteLine("Sql Deploy version 3\n");

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
                var appliedScripts = SchemaHelpers.GetScripts();
                // This loop contains horrors that cannot be unseen, one day I'll be brave enough to refactor it
                foreach (var script in appliedScripts)
                {
                    if (fileLoader.Files.ContainsKey(script.Id))
                    {
                        var file = fileLoader.Files[script.Id];
                        if (!file.Matches(script))
                        {
                            Logger.WriteLine(string.Format("WARNING! The script {0} has changed since it was applied on {1}.", file.FileName, script.Applied.Date.ToShortDateString()));
                            if (!Configuration.World.Unattended)
                            {
                                Console.WriteLine("To continue type CONTINUE at the prompt: ");
                                var option = Console.ReadLine();
                                if (option != "CONTINUE")
                                    Abort("User aborted due to a script file changing that was already applied.");
                                Console.WriteLine();
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
                Environment.Exit(1);
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
            output.AppendLine("Sql Deploy Command Line Arguments");
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
            output.AppendLine(@"SqlDeploy.exe -m Scripts -s .\SQLEXPRESS -d Northwind -i");
            output.AppendLine(@"SqlDeploy.exe -m Scripts -s .\SQLEXPRESS -d Northwind -u sa -p pa55w0rd");
            output.AppendLine("SqlDeploy.exe -m \"c:\\Example Folder\\Scripts\" -s .\\SQLEXPRESS -d Northwind -i");
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
