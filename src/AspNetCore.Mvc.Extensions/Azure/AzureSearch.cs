using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.AzureStorage
{
    public static class AzureSearchServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureSearch(this IServiceCollection services, Func<IServiceProvider, IAzureSearch> implementationFactory)
        {
            return services.AddTransient(implementationFactory);
        }
    }

    public abstract class AzureSearch : IAzureSearch
    {
        private readonly SearchIndexClient _searchIndexClient;

        public AzureSearch(string searchServiceName, string indexName, string queryApiKey)
        {
            _searchIndexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
        }

        public Task<DocumentSearchResult<Document>> SearchAsync(string searchText)
        {
            return _searchIndexClient.Documents.SearchAsync(searchText);
        }

        public Task<DocumentSearchResult<Document>> SearchAsync(string searchText, string tag, string value)
        {
            SearchParameters parameters = new SearchParameters()
            {
                Filter = $"{tag} eq '{value}'",
                QueryType = QueryType.Full
            };

            return _searchIndexClient.Documents.SearchAsync(searchText, parameters);
        }
    }
}
