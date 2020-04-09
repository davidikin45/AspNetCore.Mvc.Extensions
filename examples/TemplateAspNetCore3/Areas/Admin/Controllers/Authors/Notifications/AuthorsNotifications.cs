using AspNetCore.Mvc.Extensions.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using TemplateAspNetCore3.Dtos;

namespace TemplateAspNetCore3.Areas.Admin.Controllers.Author.Notifications
{
    public class AuthorsNotifications : ISignalRHubMap
    {
        public void MapHub(IEndpointRouteBuilder routes, string hubPathPrefix)
        {
            routes.MapHub<ApiNotificationHub<AuthorDto>>(hubPathPrefix + "/blog/authors/notifications");
        }

        public void MapHub(HubRouteBuilder routes, string hubPathPrefix)
        {
            routes.MapHub<ApiNotificationHub<AuthorDto>>(hubPathPrefix + "/blog/authors/notifications");
        }
    }
}
