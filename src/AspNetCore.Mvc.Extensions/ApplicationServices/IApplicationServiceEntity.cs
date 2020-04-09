using AspNetCore.Mvc.Extensions.DomainEvents;
using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Application
{
    public interface IApplicationServiceEntity<TCreateDto, TReadDto, TUpdateDto, TDeleteDto> : IApplicationServiceEntityReadOnly<TReadDto>
          where TCreateDto : class
          where TReadDto : class
          where TUpdateDto : class
          where TDeleteDto : class
    {

        TCreateDto GetCreateDefaultDto();

        object GetCreateDefaultCollectionItemDto(string collectionExpression);

        Result<TReadDto> Create(TCreateDto dto, string createdBy);

        Task<Result<TReadDto>> CreateAsync(TCreateDto dto, string createdBy, CancellationToken cancellationToken = default(CancellationToken));

        List<Result> BulkCreate(TCreateDto[] dtos, string createdBy);

        Task<List<Result>> BulkCreateAsync(TCreateDto[] dtos, string createdBy, CancellationToken cancellationToken = default(CancellationToken));

        Result Update(object id, TUpdateDto dto, string updatedBy);

        Task<Result> UpdateAsync(object id, TUpdateDto dto, string updatedBy, CancellationToken cancellationToken = default(CancellationToken));

        List<Result> BulkUpdate(BulkDto<TUpdateDto>[] dtos, string updatedBy);

        Task<List<Result>> BulkUpdateAsync(BulkDto<TUpdateDto>[] dtos, string updatedBy, CancellationToken cancellationToken = default(CancellationToken));

        Result UpdateGraph(object id, TUpdateDto dto, string updatedBy);

        Task<Result> UpdateGraphAsync(object id, TUpdateDto dto, string updatedBy, CancellationToken cancellationToken = default(CancellationToken));

        List<Result> BulkUpdateGraph(BulkDto<TUpdateDto>[] dtos, string updatedBy);

        Task<List<Result>> BulkUpdateGraphAsync(BulkDto<TUpdateDto>[] dtos, string updatedBy, CancellationToken cancellationToken = default(CancellationToken));

        Result UpdatePartial(object id, JsonPatchDocument dtoPatch, string updatedBy);

        List<Result> BulkUpdatePartial(BulkDto<JsonPatchDocument>[] dtoPatches, string updatedBy);

        Task<Result> UpdatePartialAsync(object id, JsonPatchDocument dtoPatch, string updatedBy, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Result>> BulkUpdatePartialAsync(BulkDto<JsonPatchDocument>[] dtoPatches, string updatedBy, CancellationToken cancellationToken = default(CancellationToken));

        Result Delete(object id, string deletedBy);

        Task<Result> DeleteAsync(object id, string deletedBy, CancellationToken cancellationToken = default(CancellationToken));

        Result Delete(TDeleteDto dto, string deletedBy);

        Task<Result> DeleteAsync(TDeleteDto dto, string deletedBy, CancellationToken cancellationToken = default(CancellationToken));

        List<Result> BulkDelete(TDeleteDto[] dtos, string deletedBy);

        Task<List<Result>> BulkDeleteAsync(TDeleteDto[] dtos, string deletedBy, CancellationToken cancellationToken = default(CancellationToken));

        TUpdateDto GetUpdateDtoById(object id);

        IEnumerable<TUpdateDto> GetUpdateDtosByIds(IEnumerable<object> ids);

        Task<TUpdateDto> GetUpdateDtoByIdAsync(object id, CancellationToken cancellationToken);

        Task<IEnumerable<TUpdateDto>> GetUpdateDtosByIdsAsync(CancellationToken cancellationToken, IEnumerable<object> ids);

        TDeleteDto GetDeleteDtoById(object id);

        IEnumerable<TDeleteDto> GetDeleteDtosByIds(IEnumerable<object> ids);

        Task<TDeleteDto> GetDeleteDtoByIdAsync(object id, CancellationToken cancellationToken);

        Task<IEnumerable<TDeleteDto>> GetDeleteDtosByIdsAsync(CancellationToken cancellationToken, IEnumerable<object> ids);        
    }
}
