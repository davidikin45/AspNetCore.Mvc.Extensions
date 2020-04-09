using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public interface IDbStartupTask : IHostedService
    {
        public int Order { get; }

    }
}
