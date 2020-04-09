using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.UI;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.ApiClient
{
    public interface IApiControllerEntityReadOnly<TReadDto>
        where TReadDto : class
    {
        Task<ActionResult<WebApiListResponseDto<TReadDto>>> Search(WebApiSearchQueryParamsDto<TReadDto> resourceParameters);

        Task<ActionResult<TReadDto>> GetById(string id, WebApiParamsDto<TReadDto> parameters);
        Task<ActionResult<TReadDto>> GetByIdFullGraph(string id, WebApiParamsDto<TReadDto> parameters);

        Task<ActionResult<List<TReadDto>>> BulkGetByIds(IEnumerable<string> ids);

        Task<IActionResult> GetByIdChildCollection(string id, string collection, WebApiSearchQueryParamsDto resourceParameters);
    }

    public interface IApiControllerEntityReadOnlyClient<TReadDto>
         where TReadDto : class
    {
        string ResourceCollection { get; }

        Task<(WebApiListResponseDto<TReadDto> data, PagingInfoDto pagingInfo)> SearchAsync(WebApiSearchQueryParamsDto resourceParameters, CancellationToken cancellationToken);

        Task<TReadDto> GetByIdAsync(object id, WebApiParamsDto parameters, CancellationToken cancellationToken);
        Task<TReadDto> GetByIdFullGraphAsync(object id, WebApiParamsDto parameters, CancellationToken cancellationToken);

        Task<List<TReadDto>> BulkGetByIdsAsync(IEnumerable<object> ids, CancellationToken cancellationToken);

        Task<(WebApiListResponseDto<TCollectionItemDto> data, PagingInfoDto pagingInfo)> GetByIdChildCollectionAsync<TCollectionItemDto>(object id, string collection, WebApiSearchQueryParamsDto resourceParameters, CancellationToken cancellationToken) where TCollectionItemDto : class;
        Task<TCollectionItemDto> GetByIdChildCollectionItemAsync<TCollectionItemDto>(object id, string collection, string collectionItemId, CancellationToken cancellationToken) where TCollectionItemDto : class;
    }
}
