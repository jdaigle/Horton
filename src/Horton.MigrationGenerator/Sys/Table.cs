using System;
using System.Collections.Generic;

namespace Horton.MigrationGenerator.Sys
{
    public class Table
    {
        public string name { get; set; }
        public int object_id { get; set; }
        public int schema_id { get; set; }

        public DateTime create_date { get; set; }

        public Schema Schema { get; internal set; }

        public List<Column> Columns { get; } = new List<Column>();

        public IList<ForeignKey> ForeignKeys { get; } = new List<ForeignKey>();
        public IList<ForeignKey> OutboundForeignKeys { get; } = new List<ForeignKey>();
        public int ForeignKeyDeptch { get; internal set; }

        public IList<Index> Indexes { get; } = new List<Index>();

        public List<CheckConstraint> TableCheckConstraints { get; } = new List<CheckConstraint>();
    }
}
