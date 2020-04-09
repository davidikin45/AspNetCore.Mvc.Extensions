using AspNetCore.Mvc.Extensions.Plugins;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions
{
    public static class AssemblyLoader
    {
        //Source: https://dotnetstories.com/blog/Dynamically-pre-load-assemblies-in-a-ASPNET-Core-or-any-C-project-en-7155735300

        public static List<Assembly> GetLoadedAssemblies()
        {
            return GetLoadedAssemblies(a => true);
        }

        public static List<Assembly> GetLoadedAssemblies(Func<Assembly, bool> predictate)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(predictate)
                .ToList();
        }

        public static List<Assembly> LoadApplicationDependencies(bool includeFramework = false)
        {
            return LoadApplicationDependencies(DependencyContext.Default);
        }

        public static List<Assembly> LoadApplicationDependencies(DependencyContext context, bool includeFramework = false)
        {
            // Storage to ensure not loading the same assembly twice and optimize calls to GetAssemblies()
            Dictionary<string, bool> loaded = new Dictionary<string, bool>();

            // Filter to avoid loading all the .net framework
            bool ShouldLoad(string assemblyName)
            {
                return (includeFramework || NotNetFramework(assemblyName))
                    && !loaded.ContainsKey(assemblyName);
            }
            bool NotNetFramework(string assemblyName)
            {
                return !assemblyName.StartsWith("Microsoft.")
                    && !assemblyName.StartsWith("System")
                    && !assemblyName.StartsWith("Newtonsoft.")
                    && !assemblyName.StartsWith("netstandard")
                    && !assemblyName.StartsWith("Remotion.Linq")
                    && !assemblyName.StartsWith("SOS.NETCore")
                    && !assemblyName.StartsWith("WindowsBase")
                    && !assemblyName.StartsWith("mscorlib");
            }

            // Populate already loaded assemblies
            System.Diagnostics.Debug.WriteLine($">> Already loaded assemblies:");
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => ShouldLoad(a.GetName().Name)))
            {
                loaded.Add(a.GetName().Name, true);
                System.Diagnostics.Debug.WriteLine($">>>> {a.FullName}");
            }
            int alreadyLoaded = loaded.Keys.Count();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            var assemblies = context.RuntimeLibraries
              .SelectMany(library => library.GetDefaultAssemblyNames(context))
              .Where(an => ShouldLoad(an.FullName))
              .Select(an => {
                  loaded.Add(an.FullName, true);
                  System.Diagnostics.Debug.WriteLine($"\n>> Referenced assembly => {an.FullName}");
                  return Assembly.Load(an);
              })
              .ToList();

            // Debug
            System.Diagnostics.Debug.WriteLine($"\n>> Assemblies loaded after scan ({(loaded.Keys.Count - alreadyLoaded)} assemblies in {sw.ElapsedMilliseconds} ms):");
            foreach (var a in loaded.Keys.OrderBy(k => k))
                System.Diagnostics.Debug.WriteLine($">>>> {a}");

            return assemblies;
        }

        public static List<Assembly> LoadAssembliesFromPath(string path)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assemblyPath in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => new[] { ".dll" }.Any(file.ToLower().EndsWith)))
            {
                System.Diagnostics.Debug.WriteLine($"Loading Assembly: {assemblyPath}");
                var assembly = Assembly.LoadFrom(assemblyPath);
                assemblies.Add(assembly);
            }

            return assemblies;
        }


        public static List<Assembly> LoadPluginAssembliesFromPath(string path)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assemblyDirectory in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                var directoryName = Path.GetDirectoryName(assemblyDirectory);
                var pluginDLLLocation = Directory.EnumerateFiles(assemblyDirectory, $"{directoryName}.dll", SearchOption.AllDirectories).FirstOrDefault();

                if (pluginDLLLocation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Loading Assembly: {pluginDLLLocation}");
                    PluginLoadContext loadContext = new PluginLoadContext(pluginDLLLocation);
                    var pluginAssembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginDLLLocation)));
                    assemblies.Add(pluginAssembly);
                }
            }

            return assemblies;
        }
    }
}

