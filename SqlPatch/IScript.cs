using System;
namespace SqlPatch {
    public interface IScript {
        Guid Id { get; }
        Guid ContentHash { get; }
        string FileName { get; }
        bool Matches(IScript other);
    }
}
