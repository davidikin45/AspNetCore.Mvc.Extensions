using AspNetCore.Base.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Base.Notifications
{
    public class CompositeNotificationService : INotificationService
    {
        private readonly IEnumerable<INotificationProviderService> _notificationServices;

        public CompositeNotificationService(IEnumerable<INotificationProviderService> notificationServices)
        {
            _notificationServices = notificationServices;
        }

        public async Task SendAsync(string message, string userId)
        {
            foreach (var notificationService in _notificationServices)
            {
                await notificationService.SendAsync(message, userId);
            }
        }
    }
}