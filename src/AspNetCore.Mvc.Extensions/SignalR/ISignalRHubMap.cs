using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.Mvc.Extensions.SignalR
{
    public interface ISignalRHubMap
    {
        //.NET Core 2.2 
        //void MapHub(HubRouteBuilder routes, string hubPathPrefix);

        void MapHub(IEndpointRouteBuilder routes, string hubPathPrefix);
    }
}
