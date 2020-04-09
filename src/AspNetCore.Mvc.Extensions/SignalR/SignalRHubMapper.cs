using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.SignalR
{
    public class SignalRHubMapper : ISignalRHubMapper
    {
        private IEnumerable<ISignalRHubMap> Maps;
        private readonly SignalRHubMapperOptions _options;
        public SignalRHubMapper(IEnumerable<ISignalRHubMap> maps, IOptions<SignalRHubMapperOptions> options)
        {
            Maps = maps;
            _options = options.Value;
        }

        //.NET Core 2.2
        //public void MapHubs(HubRouteBuilder routes)
        //{
        //    foreach (var map in Maps)
        //    {
        //        map.MapHub(routes, _options.SignalRUrlPrefix);
        //    }
        //}

        public void MapHubs(IEndpointRouteBuilder routes)
        {
            foreach (var map in Maps)
            {
                map.MapHub(routes, _options.SignalRUrlPrefix);
            }
        }
    }

    public class SignalRHubMapperOptions
    {
        public string SignalRUrlPrefix { get; set; } = "/api";

        public bool LoadApplicationDependencies { get; set; } = true;
        public string LoadPathDependencies { get; set; }
        public Func<Assembly, bool> Predicate { get; set; } = a => true;
    }

    public interface ISignalRHubMapper
    {
        //.NET Core 2.2
        //void MapHubs(HubRouteBuilder routes);

        void MapHubs(IEndpointRouteBuilder routes);
    }
}
