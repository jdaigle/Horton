using System;
using System.CodeDom.Compiler;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using Horton.MigrationGenerator.EF6;
using NDesk.Options;

namespace Horton.MigrationGenerator
{
    public sealed class AddMigrationCommand : HortonCommand
    {
        public override string Name { get { return "ADD-MIGRATION"; } }
        public override string Description { get { return "Scaffolds a new migration based on the EF6 entity model compared to the phsyical database."; } }

        public override void Execute(HortonOptions options)
        {
            string dbContextAssemblyPath = "";

            var p = new OptionSet()
            {
                { "dbcontext=", "", v => dbContextAssemblyPath = v },
            };
            p.Parse(options.ExtraParameters);

            if (string.IsNullOrEmpty(dbContextAssemblyPath) || !File.Exists(dbContextAssemblyPath))
            {
                throw new ArgumentNullException(nameof(dbContextAssemblyPath));
            }

            var asm = Assembly.LoadFrom(dbContextAssemblyPath);

            Type dbContextType = null;
            foreach (var type in asm.GetLoadableTypes())
            {
                if (typeof(DbContext).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    dbContextType = type;
                    break;
                }
            }

            if (dbContextType == null)
            {
                throw new ArgumentNullException(nameof(dbContextType));
            }

            var connectionString = options.CreateConnectionString();
            var context = Activator.CreateInstance(dbContextType, new[] { connectionString }) as DbContext;

            var changes = new DiffTool(((IObjectContextAdapter)context).ObjectContext, new SqlConnection(connectionString)).FindChanges();

            var sb = new StringBuilder();
            var textWriter = new IndentedTextWriter(new StringWriter(sb));
            foreach (var change in changes)
            {
                change.AppendDDL(textWriter);
                textWriter.WriteLine();
                textWriter.WriteLine();
            }
            var migration = sb.ToString();
        }
    }
}
