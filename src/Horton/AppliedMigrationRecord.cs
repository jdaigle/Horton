using System;

namespace Horton
{
    public class AppliedMigrationRecord
    {
        public string FileName { get; internal set; }

        public byte Type { get; internal set; }
        public string ContentHash { get; internal set; }

        public DateTime AppliedUTC { get; internal set; }
        public string SystemUser { get; internal set; }
    }
}
