using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.SignalR
{
    public interface INotificationClient
    {
        Task ReceiveMessage(string message);
    }

    public class NotificationHub : Hub<INotificationClient>
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

    public static class NotificationHubServerExtensions
    {
        public static Task SendMessageToClients(this IHubContext<NotificationHub, INotificationClient> hubContext, string message, params string[] connectionIds)
        {
            //return hubContext.Clients.Clients(connectionIds.ToList()).SendAsync("ReceiveMessage", message);
            return hubContext.Clients.Clients(connectionIds.ToList()).ReceiveMessage(message);
        }

        public static Task SendMessageToUsers(this IHubContext<NotificationHub, INotificationClient> hubContext, string message, params string[] userIds)
        {
            //return hubContext.Clients.Users(userIds.ToList()).SendAsync("ReceiveMessage", message);
            return hubContext.Clients.Users(userIds.ToList()).ReceiveMessage(message);
        }

        public static Task SendMessageToAllUsers(this IHubContext<NotificationHub, INotificationClient> hubContext, string message)
        {
            //return hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            return hubContext.Clients.All.ReceiveMessage(message);
        }

        public static Task SendMessageToGroups(this IHubContext<NotificationHub, INotificationClient> hubContext, string message, params string[] groups)
        {
            //return hubContext.Clients.Groups(groups.ToList()).SendAsync("ReceiveMessage", message);
            return hubContext.Clients.Groups(groups.ToList()).ReceiveMessage(message);
        }
    }
}
