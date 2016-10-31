using Horton.SqlServer;

namespace Horton
{
    internal class HistoryCommand : HortonCommand
    {
        public override string Name { get { return "HISTORY"; } }
        public override string Description { get { return "Prints all previously executed migrations."; } }

        public override void Execute(HortonOptions options)
        {
            using (var schemaInfo = new SchemaInfo(options))
            {
                schemaInfo.InitializeTable();

                Program.PrintLine("=== History ===");
                Program.PrintLine();
                Program.PrintLine("Timestamp (UTC)        | File Name            | User");
                Program.PrintLine("---------------------------------------------------------------------");
                foreach (var item in schemaInfo.AppliedMigrations)
                {
                    Program.PrintLine($"{item.AppliedUTC.ToString("yyyy-MM-dd HH:mm:ss.ff")} | {TrimOrPad(item.FileName, 20)} | {TrimOrPad(item.SystemUser, 20)}");
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