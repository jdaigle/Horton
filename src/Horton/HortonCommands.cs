namespace Horton
{
    public static class HortonCommands
    {
        public const string UPDATE = "update";
        public const string INFO = "info";
        public const string SYNC = "sync";
        public const string HISTORY = "history";

        public static string TryParseCommand(string command)
        {
            var cmd = command.ToLowerInvariant();

            if (cmd == UPDATE)
                return UPDATE;

            if (cmd == INFO)
                return INFO;

            if (cmd == SYNC)
                return SYNC;

            if (cmd == HISTORY)
                return HISTORY;

            return null;
        }
    }
}
