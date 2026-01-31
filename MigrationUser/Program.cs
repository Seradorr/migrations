using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Migrations;

namespace MigrationUser {
    class Program {
        static void Main(string[] args) {
            MigrationVivado m = new MigrationVivado();
            m.writeLog = Console.Write;
            //m.projectFileFA = Console.ReadLine();
            //m.targetDir = Console.ReadLine();
            m.MigrateProject();

            Console.ReadLine();
        }
    }
}
