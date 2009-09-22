using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace Cridion.SchemaMigrator
{
    public class Compiler
    {
        
        CSharpCodeProvider compiler = new CSharpCodeProvider();
        CompilerParameters compParams = new CompilerParameters();

        public Compiler()
        {
            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.ReferencedAssemblies.Add("Migrator.exe");
            compParams.ReferencedAssemblies.Add("Assemblies/Microsoft.SqlServer.ConnectionInfo.dll");
            compParams.ReferencedAssemblies.Add("Assemblies/Microsoft.SqlServer.Smo.dll");
            compParams.ReferencedAssemblies.Add("Assemblies/Microsoft.SqlServer.SqlEnum.dll");
        }

        public Migration Compile(String sourceName)
        {
                        
            CompilerResults cr = compiler.CompileAssemblyFromFile(compParams, sourceName);                        

            if (cr.Errors.Count > 0)
            {
                // Display compilation errors.
                Console.WriteLine("Errors building {0} into {1}",
                    sourceName, cr.PathToAssembly);
                foreach (CompilerError ce in cr.Errors)
                {
                    Console.WriteLine("  {0}", ce.ToString());
                    Console.WriteLine();
                }
                throw new Exception("There is PROBABLY a PROBLEM in your SOURCE file");
            }

            foreach (Type type in cr.CompiledAssembly.GetExportedTypes())
            {
                Migration migration = (Migration)cr.CompiledAssembly.CreateInstance(type.FullName);
                if (migration == null)
                    throw new Exception("Failed to create migration object: "+type.FullName);
                return migration;
            }

            throw new Exception("No migration found in this source file: " + sourceName);

        }

        

    }
}
