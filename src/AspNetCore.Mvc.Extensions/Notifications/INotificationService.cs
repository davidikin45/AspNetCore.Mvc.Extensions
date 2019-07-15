using System.Threading.Tasks;

namespace AspNetCore.Base.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(string message, string userId);
    }
}
