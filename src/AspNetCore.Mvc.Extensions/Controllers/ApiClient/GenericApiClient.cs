using AspNetCore.Mvc.Extensions.Alerts;
using AspNetCore.Mvc.Extensions.Dtos;
using GrabMobile.ApiClient.HttpClientREST;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.ApiClient
{
    public class GenericApiClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto> : GenericReadOnlyApiClient<TReadDto>, IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>
        where TCreateDto : class
        where TReadDto : class
        where TUpdateDto : class
        where TDeleteDto : class
    {
        public GenericApiClient(HttpClient client, JsonSerializerSettings settings, string resourceCollection)
            :base(client, settings, resourceCollection)
        {

        }

        #region New Instance
        public async Task<TCreateDto> NewDefaultAsync(CancellationToken cancellationToken = default)
        {
            var response = await client.Get($"{ResourceCollection}/new");

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<TCreateDto>();
        }
        #endregion

        #region Create
        public async Task<TReadDto> CreateAsync(TCreateDto dto)
        {
            var response = await client.Post($"{ResourceCollection}", dto, settings);

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<TReadDto>();
        }
        #endregion

        #region Bulk Create
        public async Task<List<ValidationProblemDetails>> BulkCreateAsync(TCreateDto[] dtos)
        {
            var response = await client.Post($"{ResourceCollection}/bulk", dtos, settings);

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<ValidationProblemDetails>>();
        }
        #endregion

        #region Get for Edit
        public async Task<TUpdateDto> GetByIdForEditAsync(object id, CancellationToken cancellationToken = default)
        {
            var response = await client.Get($"{ResourceCollection}/edit/{id}");

            TUpdateDto item = null;
            if (response.IsSuccessStatusCode)
            {
                item = await response.ContentAsTypeAsync<TUpdateDto>();
            }

            return item;
        }
        #endregion

        #region Bulk Get for Edit
        public async Task<List<TUpdateDto>> BulkGetByIdsForEditAsync(IEnumerable<object> ids)
        {
            var response = await client.Get($"{ResourceCollection}/bulk/edit/{String.Join(',', ids)}");

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<TUpdateDto>>();
        }
        #endregion

        #region Update
        public async Task UpdateAsync(object id, TUpdateDto dto)
        {
            var response = await client.Put($"{ResourceCollection}/{id}", dto, settings);
            await response.EnsureSuccessStatusCodeAsync();
        }
        #endregion

        #region Bulk Update
        public async Task<List<ValidationProblemDetails>> BulkUpdateAsync(BulkDto<TUpdateDto>[] dtos)
        {
            var response = await client.Put($"{ResourceCollection}/bulk", dtos, settings);

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<ValidationProblemDetails>>();
        }
        #endregion

        #region Partial Update
        public async Task UpdatePartialAsync(object id, JsonPatchDocument dtoPatch)
        {
            var response = await client.Patch($"{ResourceCollection}/{id}", dtoPatch, settings);

            await response.EnsureSuccessStatusCodeAsync();
        }
        #endregion

        #region Bulk Partial Update
        public async Task<List<ValidationProblemDetails>> BulkUpdatePartialAsync(BulkDto<JsonPatchDocument>[] dtos)
        {
            var response = await client.Patch($"{ResourceCollection}/bulk", dtos, settings);

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<ValidationProblemDetails>>();
        }
        #endregion

        #region Get For Delete
        public async Task<TDeleteDto> GetByIdForDeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            var response = await client.Get($"{ResourceCollection}/delete/{id}");

            TDeleteDto item = null;
            if (response.IsSuccessStatusCode)
            {
                item = await response.ContentAsTypeAsync<TDeleteDto>();
            }

            return item;
        }
        #endregion

        #region Bulk Get For Delete
        public async Task<List<TDeleteDto>> BulkGetByIdsForDeleteAsync(IEnumerable<object> ids)
        {
            var response = await client.Get($"{ResourceCollection}/bulk/delete/{String.Join(',', ids)}");

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<TDeleteDto>>();
        }
        #endregion

        #region Delete
        public async Task DeleteAsync(object id, [FromBody] TDeleteDto dto)
        {
            var response = await client.Delete($"{ResourceCollection}/{id}", dto, settings);

            await response.EnsureSuccessStatusCodeAsync();
        }
        #endregion

        #region Bulk Delete
        public async Task<List<ValidationProblemDetails>> BulkDeleteAsync([FromBody] TDeleteDto[] dtos)
        {
            var response = await client.Delete($"{ResourceCollection}/bulk", dtos, settings);

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<List<ValidationProblemDetails>>();
        }
        #endregion

        #region Child Collection Item
        public async Task<CollectionItemTypeDto> NewCollectionItemAsync<CollectionItemTypeDto>(string collection, CancellationToken cancellationToken = default)
        {
            var response = await client.Get($"{ResourceCollection}/new/{collection}");

            await response.EnsureSuccessStatusCodeAsync();

            return await response.ContentAsTypeAsync<CollectionItemTypeDto>();
        }
        #endregion
    }
}
