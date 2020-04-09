using AspNetCore.Mvc.Extensions.SignalR;
using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.DependencyInjection.Modules
{
    public class AutofacConventionsSignalRHubModule : Module
    {
        public string[] Paths;
        public Func<string, Boolean> Filter;

        protected override void Load(ContainerBuilder builder)
        {
            var filters = new List<Type>();
            filters.Add(typeof(ISignalRHubMap));

            foreach (string path in Paths)
            {
                var assemblies = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                              .Where(file => new[] { ".dll" }.Any(file.ToLower().EndsWith))
                              .Where(Filter)
                              .Select(System.Reflection.Assembly.LoadFrom);

                foreach (System.Reflection.Assembly assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                              .Where(p => p.GetInterfaces().Intersect(filters).Count() > 0)
                              .Select(p => p);

                    //  #4
                    foreach (var type in types)
                    {
                        foreach (var inter in type.GetInterfaces().Intersect(filters))
                        {
                            if (!type.IsAbstract)
                            {
                                if (!type.IsGenericType)
                                {
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
