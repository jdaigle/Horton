using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace Cridion.SchemaMigrator
{
    class Program
    {

        static void Main(string[] args)
        {                        

            /* target version from command line */
            int TargetVersion = -1;
            if (args.Length > 0)
                TargetVersion = Int32.Parse(args[0]);

            Configuration _Configuration = Configuration.LoadConfig("Config.xml");

            //Console.WriteLine("Are you sure you want to migrate? Type \"Change\":");
            // if (!Console.ReadLine().Equals("Change"))
            //    return;

            Migrator _Migrator = new Migrator(_Configuration);
            _Migrator.Migrate(TargetVersion);

            Console.WriteLine("\nOperation Complete\n");
        }

       
    }
}
