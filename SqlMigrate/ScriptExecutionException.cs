using System;

namespace SqlMigrate {
    public class ScriptExecutionException : Exception {
        
        public string Command { get; private set; }

        public ScriptExecutionException(string command) {
            Command = command;
        }
        public ScriptExecutionException(string command, string message)
            : base(message) {
            Command = command;
        }
        public ScriptExecutionException(string command, string message, Exception innerException)
            : base(message, innerException) {
            Command = command;
        }
    }
}
