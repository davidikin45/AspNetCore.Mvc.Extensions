using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public interface IStartupTask : IHostedService
    {
        public int Order { get; }
    }
}
