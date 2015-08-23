using System;

namespace SqlMigrate {
    public class AppliedScript : IScript {

        public AppliedScript(Guid id, Guid hash, DateTime applied, string file, ScriptType type) {
            Id = id;
            ContentHash = hash;
            Applied = applied;
            FileName = file;
            Type = type;
        }

        public Guid Id { get; private set; }
        public Guid ContentHash { get; private set; }
        public DateTime Applied { get; private set; }
        public string FileName { get; private set; }
        public ScriptType Type { get; private set; }

        public bool Matches(IScript other) {
            return ContentHash.Equals(other);
        }
    }
}
