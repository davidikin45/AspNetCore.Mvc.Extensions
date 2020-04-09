using AspNetCore.Mvc.Extensions.StartupTasks;
using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.DependencyInjection.Modules
{
    public class AutofacTasksModule : Module
    {

        public string[] Paths;
        public Func<string, Boolean> Filter;

        protected override void Load(ContainerBuilder builder)
        {
            var tasks = new List<Type>();
            tasks.Add(typeof(IDbStartupTask));
            tasks.Add(typeof(IStartupTask));

            var added = new HashSet<string>();

            foreach (string path in Paths)
            {
                var assemblies = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                              .Where(file => new[] { ".dll" }.Any(file.ToLower().EndsWith))
                              .Where(Filter)
                              .Select(System.Reflection.Assembly.LoadFrom);

                foreach (System.Reflection.Assembly assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                              .Where(p => p.GetInterfaces().Intersect(tasks).Count() > 0)
                              .Select(p => p);

                    //  #4
                    foreach (var type in types)
                    {
                        foreach (var inter in type.GetInterfaces().Intersect(tasks))
                        {
                            if (!type.IsAbstract)
                            {
                                if (!type.IsGenericType && !added.Contains(type.FullName+inter.FullName))
                                {
                                    added.Add(type.FullName+inter.FullName);
                                    builder.RegisterType(type).As(inter);
                                }
                            }
                        }                      
                    
                    }
                }

            }
        }
    }


}
