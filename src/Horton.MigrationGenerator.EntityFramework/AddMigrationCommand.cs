using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horton.MigrationGenerator.EF6
{
    public sealed class AddMigrationCommand : HortonCommand
    {
        public override string Name { get { return "ADD-MIGRATION"; } }
        public override string Description { get { return "Scaffolds a new migration based on the EF6 entity model compared to the phsyical database."; } }

        public override void Execute(HortonOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
