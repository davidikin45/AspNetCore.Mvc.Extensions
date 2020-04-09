using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Features
{
    public class SessionSessionManager : ISessionManager
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public SessionSessionManager(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Task<bool?> GetAsync(string featureName)
        {
            var session = _contextAccessor.HttpContext.Session;
            var sessionKey = $"feature_{featureName}";
            if (session.TryGetValue(sessionKey, out var enabledBytes))
            {
                return Task.FromResult<bool?>(enabledBytes[0] == 1);

            }

            return Task.FromResult<bool?>(false);
        }

        public Task SetAsync(string featureName, bool enabled)
        {
            var session = _contextAccessor.HttpContext.Session;
            var sessionKey = $"feature_{featureName}";
            session.Set(sessionKey, new[] { enabled ? (byte)1 : (byte)0 });
            return Task.CompletedTask;
        }
    }
}
