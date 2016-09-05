using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horton.MigrationGenerator
{
    public sealed class CreateSchema : AbstractDatabaseChange
    {
        public CreateSchema(string schemaName)
        {
            this.SchemaName = schemaName;
        }

        private string SchemaName { get; }
    }
}
