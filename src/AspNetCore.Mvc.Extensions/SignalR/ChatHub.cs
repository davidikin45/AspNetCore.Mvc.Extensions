using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.SignalR
{
    public interface IChatClient
    {
        Task ReceiveMessage(string message);
    }

	public class ChatHub : Hub<IChatClient>
	{
		//Client JS RPC methods

		public Task SendMessageToClients(string message, params string[] connectionIds)
		{
			//return Clients.Clients(connectionIds.ToList()).SendAsync("ReceiveMessage", message);
			return Clients.Clients(connectionIds.ToList()).ReceiveMessage(message);
		}

		public Task SendMessageToUsers(string message, params string[] userIds)
		{
			//return Clients.Users(userIds.ToList()).SendAsync("ReceiveMessage", message);
			return Clients.Users(userIds.ToList()).ReceiveMessage(message);
		}

		public Task SendMessageToAllUsers(string message)
		{
			//return Clients.All.SendAsync("ReceiveMessage", message);
			return Clients.All.ReceiveMessage(message);
		}

		public Task SendMessageToGroups(string message, params string[] groups)
		{
			//return Clients.Groups(groups.ToList()).SendAsync("ReceiveMessage", message);
			return Clients.Groups(groups.ToList()).ReceiveMessage(message);
		}
		public Task SendMessageBackToSender(string message)
		{
			//return Clients.Caller.SendAsync("ReceiveMessage", message);
			return Clients.Caller.ReceiveMessage(message);
		}

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

	public static class ChatHubServerExtensions
	{
		public static Task SendMessageToClients(this IHubContext<ChatHub, IChatClient> hubContext, string message, params string[] connectionIds)
		{
			//return hubContext.Clients.Clients(connectionIds.ToList()).SendAsync("ReceiveMessage", message);
			return hubContext.Clients.Clients(connectionIds.ToList()).ReceiveMessage(message);
		}

		public static Task SendMessageToUsers(this IHubContext<ChatHub, IChatClient> hubContext, string message, params string[] userIds)
		{
			//return hubContext.Clients.Users(userIds.ToList()).SendAsync("ReceiveMessage", message);
			return hubContext.Clients.Users(userIds.ToList()).ReceiveMessage(message);
		}

		public static Task SendMessageToAllUsers(this IHubContext<ChatHub, IChatClient> hubContext, string message)
		{
			//return hubContext.Clients.All.SendAsync("ReceiveMessage", message);
			return hubContext.Clients.All.ReceiveMessage(message);
		}

		public static Task SendMessageToGroups(this IHubContext<ChatHub, IChatClient> hubContext, string message, params string[] groups)
		{
			//return hubContext.Clients.Groups(groups.ToList()).SendAsync("ReceiveMessage", message);
			return hubContext.Clients.Groups(groups.ToList()).ReceiveMessage(message);
		}
	}
}
