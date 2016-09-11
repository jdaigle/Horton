namespace Horton.MigrationGenerator
{
    public sealed class HortonPlugin : IPlugin
    {
        public void Load()
        {
            HortonCommands.RegisterCommand(new AddMigrationCommand());
            HortonCommands.RegisterCommand(new DumpSchemaCommand());
        }
    }
}
