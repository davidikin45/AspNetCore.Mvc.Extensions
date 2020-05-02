using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.UI;
using AspNetCore.Specification.UI;
using GrabMobile.ApiClient.HttpClientREST;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.ApiClient
{
    //https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
    //HttpClient does not throw an exception when the HTTP response contains an error code.Instead, the IsSuccessStatusCode property is false if the status is an error code.If you prefer to treat HTTP error codes as exceptions, call HttpResponseMessage.EnsureSuccessStatusCode on the response object. EnsureSuccessStatusCode throws an exception if the status code falls outside the range 200–299. Note that HttpClient can throw exceptions for other reasons — for example, if the request times out.
    public class GenericReadOnlyApiClient<TReadDto> : IApiControllerEntityReadOnlyClient<TReadDto>
       where TReadDto : class
    {
        protected readonly HttpClient client;
        public string ResourceCollection { get; }
        protected readonly JsonSerializerSettings settings;

        public GenericReadOnlyApiClient(HttpClient client, JsonSerializerSettings settings, string resourceCollection)
        {
            this.client = client;
            ResourceCollection = resourceCollection;
            this.settings = settings;
        }

        #region Search
        public async Task<(WebApiListResponseDto<TReadDto> data, PagingInfoDto pagingInfo)> SearchAsync(WebApiSearchQueryParamsDto resourceParameters = null, CancellationToken cancellationToken = default)
        {
            var response = await client.GetWithQueryString($"{ResourceCollection}", resourceParameters);

            await response.EnsureSuccessStatusCodeAsync();

            return (await response.ContentAsTypeAsync<WebApiListResponseDto<TReadDto>>(), response.Headers.FindAndParsePagingInfo());
        }
        #endregion

        #region GetById
        public async Task<TReadDto> GetByIdAsync(object id, WebApiParamsDto parameters = null, CancellationToken cancellationToken = default)
        {
            var response = await client.GetWithQueryString($"{ResourceCollection}/{id}", parameters);

            TReadDto item = null;
            if (response.IsSuccessStatusCode)
            {
                item = await response.ContentAsTypeAsync<TReadDto>();
            }

            return item;
        }

        public async Task<List<TReadDto>> BulkGetByIdsAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            var response = await client.Get($"{ResourceCollection}/{String.Join(',', ids)}");

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<TReadDto>>();
        }

        public async Task<TReadDto> GetByIdFullGraphAsync(object id, WebApiParamsDto parameters = null, CancellationToken cancellationToken = default)
        {
            var response = await client.GetWithQueryString($"{ResourceCollection}/full-graph/{id}", parameters);

            TReadDto item = null;
            if (response.IsSuccessStatusCode)
            {
                item = await response.ContentAsTypeAsync<TReadDto>();
            }

            return item;
        }
        #endregion

        #region Child Collection
        public async Task<(WebApiListResponseDto<TChildCollectionItemDto> data, PagingInfoDto pagingInfo)> GetByIdChildCollectionAsync<TChildCollectionItemDto>(object id, string collection, WebApiSearchQueryParamsDto resourceParameters, CancellationToken cancellationToken = default)
     where TChildCollectionItemDto : class
        {
            var response = await client.GetWithQueryString($"{ResourceCollection}/{id}/{collection}", resourceParameters);

            await response.EnsureSuccessStatusCodeAsync();

            return (await response.ContentAsTypeAsync<WebApiListResponseDto<TChildCollectionItemDto>>(), response.Headers.FindAndParsePagingInfo());
        }

        public async Task<TChildCollectionItemDto> GetByIdChildCollectionItemAsync<TChildCollectionItemDto>(object id, string collection, string collectionItemId, CancellationToken cancellationToken = default)
            where TChildCollectionItemDto : class
        {
            var response = await client.Get($"{ResourceCollection}/{id}/{collection}/{collectionItemId}");

            TChildCollectionItemDto item = null;
            if (response.IsSuccessStatusCode)
            {
                item = await response.ContentAsTypeAsync<TChildCollectionItemDto>();
            }

            return item;
        }
        #endregion
    }
}