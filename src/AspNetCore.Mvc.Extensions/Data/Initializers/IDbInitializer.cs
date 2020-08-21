using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Initializers
{
    public interface IDbInitializer
    {
        Task InitializeAsync(string connectionString, CancellationToken cancellationToken);
        Task InitializeSchemaAsync(string connectionString, CancellationToken cancellationToken);
    }
}
