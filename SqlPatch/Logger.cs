using System;

namespace SqlPatch {
    public static class Logger {

        private static int indentLevel = 0;
        
        public static void WriteLine(string value) {
            for (int i = 0; i < indentLevel; i++) {
                Console.Write("\t");
            }
            Console.WriteLine(value);
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
