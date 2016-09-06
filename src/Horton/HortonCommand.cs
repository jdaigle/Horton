namespace Horton
{
    public abstract class HortonCommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract void Execute(HortonOptions options);
    }
}