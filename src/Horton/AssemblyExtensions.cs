using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Horton
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static IEnumerable<Assembly> GetAssembliesInDirectory(string path)
        {
            foreach (var a in GetAssembliesInDirectoryWithExtension(path, "*.exe"))
            {
                yield return a;
            }
            foreach (var a in GetAssembliesInDirectoryWithExtension(path, "*.dll"))
            {
                yield return a;
            }
        }

        public static IEnumerable<Assembly> GetAssembliesInDirectoryWithExtension(string path, string extension)
        {
            var result = new List<Assembly>();
            foreach (var file in new DirectoryInfo(path).GetFiles(extension, SearchOption.AllDirectories))
            {
                try
                {
                    result.Add(Assembly.LoadFrom(file.FullName));
                }
                catch (BadImageFormatException)
                {
                    // skip it
                }
            }
            return result;
        }
    }
}
