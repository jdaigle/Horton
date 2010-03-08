using System;

namespace SqlPatch {
    public class AppliedScript : IScript {

        public AppliedScript(Guid id, Guid hash, DateTime applied, string file) {
            Id = id;
            ContentHash = hash;
            Applied = applied;
            FileName = file;
        }

        public Guid Id { get; private set; }
        public Guid ContentHash { get; private set; }
        public DateTime Applied { get; private set; }
        public string FileName { get; private set; }

        public bool Matches(IScript other) {
            return ContentHash.Equals(other);
        }

    }
}
