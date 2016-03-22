using System;
using Horton.SqlServer;

namespace Horton
{
    internal class HistoryCommand : HortonCommand
    {
        public override void Execute(HortonOptions options)
        {
            using (var schemaInfo = new SchemaInfo(options))
            {
                schemaInfo.InitializeTable();

                Console.WriteLine("=== History ===");
                Console.WriteLine();
                Console.WriteLine("Timestamp (UTC)        | File Name            | User");
                Console.WriteLine("---------------------------------------------------------------------");
                foreach (var item in schemaInfo.AppliedMigrations)
                {
                    Console.WriteLine($"{item.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")} | {TrimOrPad(item.FileName, 20)} | {TrimOrPad(item.SystemUser, 20)}");
                }
            }
        }

        private static string TrimOrPad(string value, int length)
        {
            if (value.Length < length)
                return value.PadRight(length);
            if (value.Length > length)
                return value.Substring(0, length - 3) + "...";
            return value;
        }
    }
}