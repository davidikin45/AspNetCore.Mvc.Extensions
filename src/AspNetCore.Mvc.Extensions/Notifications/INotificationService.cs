using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(string message, string userId);
    }
}
