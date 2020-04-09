using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Initializers
{
    public interface IDbInitializer
    {
        Task InitializeAsync(string connectionString, CancellationToken cancellationToken);
        Task InitializeSchemaAsync(string connectionString, CancellationToken cancellationToken);
        Task InitializeDataAsync(string context, string tenantId, CancellationToken cancellationToken);

        void Seed(string connectionString, string tenantId);
    }
}
