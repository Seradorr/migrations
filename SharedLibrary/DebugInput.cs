using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using SimpleJSON;

namespace Migrations {
    static class DebugInput {
        static readonly string FILE = "debug_inputs.json";

        public static bool Available() {
            return File.Exists(FILE);
        }

        public static MigrationInput Get(string migrationType) {
            var d = JSONDecoder.Decode(File.ReadAllText(FILE)).ObjectValue[migrationType].ObjectValue;
            int i = d["index"].IntValue;
            var d2 = d["inputs"].ArrayValue.ElementAt(i);

            switch (migrationType) {
                case "vivado":
                    return JSONOps<MigrationInput>.Decode(d2);
                default:
                    return null;
            }
        }
    }
}
