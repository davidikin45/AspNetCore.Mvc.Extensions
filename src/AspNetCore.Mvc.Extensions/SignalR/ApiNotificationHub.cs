using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.SignalR
{
    public class ApiNotificationHub<TDto> : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var roles = Context.User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                       .Select(c => c.Value)
                       .ToList();

            foreach (var role in roles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var roles = Context.User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                     .Select(c => c.Value)
                     .ToList();

            foreach (var role in roles)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

    public static class ApiNotificationHubServerExtensions
    {
        public static async Task CreatedAsync<TDto>(this IHubContext<ApiNotificationHub<TDto>> hubContext, object dto)
        {
            await hubContext.Clients.All.SendAsync("Created", dto);
        }

        public static async Task UpdatedAsync<TDto>(this IHubContext<ApiNotificationHub<TDto>> hubContext, object dto)
        {
            await hubContext.Clients.All.SendAsync("Updated", dto);
        }

        public static async Task DeletedAsync<TDto>(this IHubContext<ApiNotificationHub<TDto>> hubContext, object dto)
        {
            await hubContext.Clients.All.SendAsync("Deleted", dto);
        }
    }
}
