using System;
namespace SqlMigrate {
    public interface IScript {
        Guid Id { get; }
        Guid ContentHash { get; }
        string FileName { get; }
        ScriptType Type { get; }
        bool Matches(IScript other);
    }

    public enum ScriptType : int {
        ChangeScript = 1,
        DatabaseObject = 2,
    }
}
