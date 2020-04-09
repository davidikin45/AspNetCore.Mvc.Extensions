using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Mapping
{
    public class AutoMapperConfiguration
    {
        public AutoMapperConfiguration(IMapperConfigurationExpression mapperConfiguration, Func<Assembly, bool> assemblyFilter = null)
        {
            Configure(mapperConfiguration, assemblyFilter);
        }
        public void Configure(IMapperConfigurationExpression mapperConfiguration, Func<Assembly, bool> assemblyFilter = null)
        {
            //mapperConfiguration.AddProfile(new UserProfileMapping(mapperConfiguration));
            RegisterMappings(mapperConfiguration, assemblyFilter);
        }

        public static void RegisterMappings(IMapperConfigurationExpression cfg, Func<Assembly, bool> assemblyFilter = null)
        {

            Func<Assembly, bool> loadAllFilter = (x => true);

            var assembliesToLoad = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assemblyFilter ?? loadAllFilter)
                .Select(a => Assembly.Load(a.GetName()))
                .ToList();

            LoadMapsFromAssemblies(cfg, assembliesToLoad.ToArray());
        }

        public static void LoadMapsFromAssemblies(IMapperConfigurationExpression cfg, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(a => a.GetExportedTypes()).ToArray();

            Load(cfg, types);
        }

        private static void Load(IMapperConfigurationExpression cfg, Type[] types)
        {
            LoadIMapFromMappings(cfg, types);
            LoadIMapToMappings(cfg, types);

            LoadCustomMappings(cfg, types);
        }

        private static void LoadCustomMappings(IMapperConfigurationExpression cfg, IEnumerable<Type> types)
        {
            var maps = (from t in types
                        where t.GetInterfaces().Count(i => typeof(IHaveCustomMappings).IsAssignableFrom(t) &&
                              !t.IsAbstract &&
                              !t.IsInterface) > 0
                        select (IHaveCustomMappings)Activator.CreateInstance(t)).ToArray();

            foreach (var map in maps)
            {
                map.CreateMappings(cfg);
            }
        }

        private static void LoadIMapFromMappings(IMapperConfigurationExpression cfg, IEnumerable<Type> types)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                              !t.IsAbstract &&
                              !t.IsInterface
                        select new
                        {
                            Source = i.GetGenericArguments()[0],
                            Destination = t
                        }).ToArray();

            foreach (var map in maps)
            {
                //var mappingExpression = cfg.CreateMap(map.Source, map.Destination);

                var createMapMethod = typeof(IProfileExpression).GetMethod(nameof(IProfileExpression.CreateMap), new List<Type>() { }.ToArray()).MakeGenericMethod(map.Source, map.Destination);
                var mappingExpression = createMapMethod.Invoke(cfg, new List<Object>() { }.ToArray());
            }
        }

        private static void LoadIMapToMappings(IMapperConfigurationExpression cfg, IEnumerable<Type> types)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapTo<>) &&
                              !t.IsAbstract &&
                              !t.IsInterface
                        select new
                        {
                            Destination = i.GetGenericArguments()[0],
                            Source = t
                        }).ToArray();

            foreach (var map in maps)
            {
                // var mappingExpression = cfg.CreateMap(map.Source, map.Destination);
                var createMapMethod = typeof(IProfileExpression).GetMethod(nameof(IProfileExpression.CreateMap), new List<Type>() { }.ToArray()).MakeGenericMethod(map.Source, map.Destination);
                var mappingExpression = createMapMethod.Invoke(cfg, new List<Object>() { }.ToArray());
            }
        }
    }
}
