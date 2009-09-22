using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace Cridion.SchemaMigrator
{
    public class Configuration
    {

        public Boolean UsesSMO
        {
            get;
            set;
        }

        
        public Boolean UsesSQL
        {
            get;
            set;
        }

        
        public String MigrationDirectory
        {
            get;
            set;
        }



        public String ConnectionString
        {
            get;
            set;
        }

        public Configuration()
        {
            UsesSMO = false;
            UsesSQL = true;
            MigrationDirectory = ".";
            ConnectionString = ".";
        }

        public static Configuration CurrentConfig { get; set; }


        public static Configuration GlobalConfig
        {
            get;
            set;
        }

        public static void SaveConfig(String File, Configuration Configuration)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(Configuration.GetType());
            x.Serialize(new FileStream(File, FileMode.OpenOrCreate, FileAccess.ReadWrite), Configuration);
        }

        public static Configuration LoadConfig(String File)
        {
            Configuration c = new Configuration();
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(c.GetType());
            GlobalConfig = (Configuration)x.Deserialize(new FileStream(File, FileMode.OpenOrCreate, FileAccess.ReadWrite));
            CurrentConfig = GlobalConfig;
            return GlobalConfig;
        }

        

    }
}
