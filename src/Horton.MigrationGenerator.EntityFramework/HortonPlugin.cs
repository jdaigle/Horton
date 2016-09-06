namespace Horton.MigrationGenerator.EF6
{
    public sealed class HortonPlugin : IPlugin
    {
        public void Load()
        {
            HortonCommands.RegisterCommand(new AddMigrationCommand());
        }
    }
}
