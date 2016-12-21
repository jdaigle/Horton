using System;
using System.Collections.Generic;
using System.Linq;

namespace Horton
{
    public static class HortonCommands
    {
        static HortonCommands()
        {
            RegisterCommand(new UpdateCommand());
            RegisterCommand(new InfoCommand());
            RegisterCommand(new SyncCommand());
            RegisterCommand(new HistoryCommand());
            RegisterCommand(new AddMigrationCommand());
        }

        private static readonly Dictionary<string, HortonCommand> _commandRegistery = new Dictionary<string, HortonCommand>(StringComparer.OrdinalIgnoreCase);

        public static ICollection<HortonCommand> Commands => _commandRegistery.Values;

        public static HortonCommand TryParseCommand(string command)
        {
            if (_commandRegistery.ContainsKey(command))
            {
                return _commandRegistery[command];
            }

            LoadPluginsLazy();
            if (_commandRegistery.ContainsKey(command))
            {
                return _commandRegistery[command];
            }

            return null;
        }

        private static bool _pluginsLoaded = false;

        public static void LoadPluginsLazy()
        {
            if (_pluginsLoaded)
            {
                return;
            }

            var assembliesDir = AppDomain.CurrentDomain.BaseDirectory;
            var thisAssembly = typeof(HortonCommands).Assembly;
            var allAssemblies = AssemblyExtensions.GetAssembliesInDirectory(assembliesDir).Distinct();
            foreach (var assembly in allAssemblies)
            {
                if (assembly == thisAssembly)
                {
                    continue;
                }

                foreach (var type in assembly.GetLoadableTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t)))
                {
                    (Activator.CreateInstance(type) as IPlugin).Load();
                }
            }


            _pluginsLoaded = true;
        }

        public static void RegisterCommand(HortonCommand command) => _commandRegistery[command.Name] = command;
    }
}
