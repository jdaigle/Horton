namespace Horton
{
    public static class HortonCommands
    {
        public const string UPDATE = "update";
        public const string INFO = "info";
        public const string SYNC = "sync";
        public const string HISTORY = "history";

        public static HortonCommand TryParseCommand(string command)
        {
            var cmd = command.ToLowerInvariant();

            if (cmd == UPDATE)
                return new UpdateCommand();

            if (cmd == INFO)
                return new InfoCommand();

            if (cmd == SYNC)
                return new SyncCommand();

            if (cmd == HISTORY)
                return new HistoryCommand();

            return null;
        }
    }
}
