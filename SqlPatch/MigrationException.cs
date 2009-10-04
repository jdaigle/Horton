using System;

namespace SchemaMigrator
{
    public class MigrationException : Exception
    {
        public string Command { get; private set; }

        public MigrationException(string command)
        {
            Command = command;
        }
        public MigrationException(string command, string message)
            : base(message)
        {
            Command = command;
        }
        public MigrationException(string command, string message, Exception innerException)
            : base(message, innerException)
        {
            Command = command;
        }
    }
}
