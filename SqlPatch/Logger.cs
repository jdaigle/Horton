using System;
using System.IO;

namespace SqlPatch {
    public static class Logger {

        private static string logname;

        static Logger() {
            logname = "SqlPatch." + DateTime.UtcNow.Ticks.ToString() + ".log";
        }

        private static int indentLevel = 0;

        public static void WriteLine(string value) {
            for (int i = 0; i < indentLevel; i++) {
                Console.Write("\t");
                File.AppendAllText(logname, "\t");
            }
            Console.WriteLine(value);
            File.AppendAllText(logname, value + "\n");
        }

        public static void Indent() {
            indentLevel++;
        }

        public static void Unindent() {
            indentLevel--;
            if (indentLevel < 0)
                indentLevel = 0;
        }

        public static void Reset() {
            indentLevel = 0;
        }
    }
}
