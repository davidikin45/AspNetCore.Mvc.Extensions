using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Reflection
{
    public class AssemblyProvider : IAssemblyProvider
    {
        private AssemblyProviderOptions Options;
        public AssemblyProvider(AssemblyProviderOptions options)
        {
            Options = options;
        }

        public IEnumerable<Assembly> GetAssemblies(IEnumerable<string> paths = null, Func<string, Boolean> filter = null)
        {
            if (paths == null)
            {
                paths = new List<string>() { Options.BinPath };
                filter = Options.AssemblyFilter;
            }

            return GetAssembliesFromPaths(paths, filter);
        }

        private IEnumerable<Assembly> GetAssembliesFromPaths(IEnumerable<string> paths, Func<string, Boolean> filter = null)
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach (string path in paths)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(path, "*.*")
                     .Where(file => new[] { ".dll" }.Any(file.ToLower().EndsWith)).ToList();

                if (filter != null)
                {
                    files = files.Where(filter);
                }

                assemblies.AddRange(files.Select(System.Reflection.Assembly.LoadFrom));
            }

            return assemblies;
        }
    }
}
