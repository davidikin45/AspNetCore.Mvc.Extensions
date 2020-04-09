using AspNetCore.Mvc.Extensions.Application;
using AspNetCore.Mvc.Extensions.Attributes.ValueProviders;
using AspNetCore.Mvc.Extensions.Authentication;
using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Controllers.ApiClient;
using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    //Edit returns a view of the resource being edited, the Update updates the resource it self

    //C - Create - POST
    //R - Read - GET
    //U - Update - PUT
    //D - Delete - DELETE

    //If there is an attribute applied(via[HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
    //If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
    //Otherwise, the action supports the POST method.

    //AuthorizeAttributes are AND not OR.
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + BasicAuthenticationDefaults.AuthenticationScheme)]
    public abstract class ApiControllerEntityAuthorizeBase<TCreateDto, TReadDto, TUpdateDto, TDeleteDto, IEntityService> : ApiControllerEntityReadOnlyAuthorizeBase<TReadDto, IEntityService>, IApiControllerEntity<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>
         where TCreateDto : class
         where TReadDto : class
         where TUpdateDto : class
         where TDeleteDto : class
        where IEntityService : IApplicationServiceEntity<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>
    {
        public ApiControllerEntityAuthorizeBase(ControllerServicesContext context, IEntityService service)
        : base(context, service)
        {

        }

        #region New Instance
        [Route("new")]
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Create)]
        [HttpGet]
        public virtual ActionResult<TCreateDto> NewDefault()
        {

            var response = Service.GetCreateDefaultDto();

            var links = CreateLinksForCreate();

            var linkedResourceToReturn = response.ShapeData("")
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }
        #endregion

        #region Create
        //[Route("create")]
        /// <summary>
        /// Creates the specified dto.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Create)]
        [HttpPost]
        public virtual async Task<ActionResult<TReadDto>> Create([FromBody] TCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrors(ModelState);
            }

            var cts = TaskHelper.CreateNewCancellationTokenSource();

            var result = await Service.CreateAsync(dto, UserId, cts.Token);
            if (result.IsFailure)
            {
                return ValidationErrors(result);
            }
            var createdDto = result.Value;

            //return CreatedAtRoute("", new { id = createdDto.Id }, createdDto);

            //return ApiSuccessMessage(Messages.AddSuccessful, createdDto.Id);
            //return Success(createdDto);

            if (createdDto.HasProperty("Id"))
            {
                return CreatedAtAction(nameof(Create), new { id = createdDto.GetPropValue("Id") }, createdDto);
            }
            else
            {
                return Ok(createdDto);
            }

        }
        #endregion

        #region Bulk Create
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Create)]
        [Route("bulk")]
        [HttpPost]
        public virtual async Task<ActionResult<List<ValidationProblemDetails>>> BulkCreate([FromBody] TCreateDto[] dtos)
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            var results = await Service.BulkCreateAsync(dtos, UserId, cts.Token);

            return BulkCreateResponse(results);
        }
        #endregion

        #region Get for Edit

        /// <summary>
        /// Ges for edit.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Update)]
        [FormatFilter]
        [Route("edit/{id}"), Route("edit/{id}.{format}")]
        //[Route("get/{id}"), Route("get/{id}.{format}")]
        [HttpGet]
        public virtual async Task<ActionResult<TUpdateDto>> GetByIdForEdit(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            //By passing true we include the Composition properties which should be any child or join entities.
            var response = await Service.GetUpdateDtoByIdAsync(id, cts.Token);

            if (response == null)
            {
                return NotFound();
            }

            var links = CreateLinks(id, "");

            var linkedResourceToReturn = response.ShapeData("")
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }
        #endregion

        #region Bulk Get for Edit
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Update)]
        [FormatFilter]
        [Route("edit/({ids})"), Route("edit/({ids}).{format}")]
        [Route("bulk/edit/({ids})"), Route("bulk/edit/({ids}).{format}")]
        [HttpGet]
        [DelimitedQueryString(',', '|')]
        public virtual async Task<ActionResult<List<TUpdateDto>>> BulkGetByIdsForEditAsync(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var response = await Service.GetUpdateDtosByIdsAsync(cts.Token, ids);

            var list = response.ToList();

            if (ids.Count() != list.Count())
            {
                return NotFound();
            }

            return Ok(list);
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Update)]
        [Route("{id}")]
        [HttpPut]
        //[HttpPost]
        public virtual async Task<IActionResult> Update(string id, [FromBody] TUpdateDto dto)
        {
            if (dto.HasProperty("Id"))
            {
                if (dto == null || id.ToString() != dto.GetPropValue("Id").ToString())
                {
                    return BadRequest();
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrors(ModelState);
            }

            var cts = TaskHelper.CreateNewCancellationTokenSource();

            //var exists = await Service.ExistsAsync(cts.Token, id);

            //if (!exists)
            //{
            //    return ApiNotFoundErrorMessage(Messages.NotFound);
            //}

            var result = await Service.UpdateGraphAsync(id, dto, UserId, cts.Token);
            if (result.IsFailure)
            {
                return ValidationErrors(result);
            }
            //return ApiSuccessMessage(Messages.UpdateSuccessful, dto.Id);
            //return Success(dto);
            return NoContent();
        }
        #endregion

        #region Bulk Update
        /// <summary>
        /// Bulks the update.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Update)]
        [Route("bulk")]
        [HttpPut]
        public virtual async Task<ActionResult<List<ValidationProblemDetails>>> BulkUpdate([FromBody] BulkDto<TUpdateDto>[] dtos)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var results = await Service.BulkUpdateGraphAsync(dtos, UserId, cts.Token);

            return BulkUpdateResponse(results);
        }
        #endregion

        #region Update Partial
        /// <summary>
        /// Updates the partial.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dtoPatch">The dto patch.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Update)]
        [Route("{id}")]
        [HttpPatch]
        public virtual async Task<IActionResult> UpdatePartial(string id, [FromBody] JsonPatchDocument dtoPatch)
        {
            if (dtoPatch == null)
            {
                return BadRequest();
            }

            var cts = TaskHelper.CreateNewCancellationTokenSource();

            var result = await Service.UpdatePartialAsync(id, dtoPatch, UserId, cts.Token);
            if (result.IsFailure)
            {
                return ValidationErrors(result);
            }

            return NoContent();
        }
        #endregion

        #region Bulk Partial Update
        /// <summary>
        /// Bulks the update.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Update)]
        [Route("bulk")]
        [HttpPatch]
        public virtual async Task<ActionResult<List<ValidationProblemDetails>>> BulkUpdatePartial([FromBody] BulkDto<JsonPatchDocument>[] dtos)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var results = await Service.BulkUpdatePartialAsync(dtos, UserId, cts.Token);

            return BulkUpdateResponse(results);
        }
        #endregion

        #region Get for Delete
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Delete, ResourceCollectionsCore.CRUD.Operations.DeleteOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Delete)]
        [FormatFilter]
        [Route("Delete/{id}"), Route("Delete/{id}.{format}")]
        [HttpGet]
        public virtual async Task<ActionResult<TDeleteDto>> GetByIdForDelete(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            //By passing true we include the Composition properties which should be any child or join entities.
            var response = await Service.GetDeleteDtoByIdAsync(id, cts.Token);

            if (response == null)
            {
                return NotFound();
            }

            var links = CreateLinks(id, "");

            var linkedResourceToReturn = response.ShapeData("")
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }
        #endregion

        #region Bulk Get for Delete
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Delete, ResourceCollectionsCore.CRUD.Operations.DeleteOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Delete)]
        [FormatFilter]
        [Route("delete/({ids})"), Route("delete/({ids}).{format}")]
        [Route("bulk/delete/({ids})"), Route("bulk/delete/({ids}).{format}")]
        [HttpGet]
        [DelimitedQueryString(',', '|')]
        public virtual async Task<ActionResult<List<TDeleteDto>>> BulkGetByIdsForDeleteAsync(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var response = await Service.GetDeleteDtosByIdsAsync(cts.Token, ids);

            var list = response.ToList();

            if (ids.Count() != list.Count())
            {
                return NotFound();
            }

            return Ok(list);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the specified dto.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Delete, ResourceCollectionsCore.CRUD.Operations.DeleteOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Delete)]
        [Route("{id}")]
        [HttpDelete]
        //[HttpPost]
        public virtual async Task<IActionResult> DeleteDto(string id, [FromBody] TDeleteDto dto)
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            if (dto == null)
            {
                return BadRequest();
            }

            if(dto.HasProperty("Id"))
            {
                if (id.ToString() != dto.GetPropValue("Id").ToString())
                {
                    return BadRequest();
                }
            }

            var result = await Service.DeleteAsync(dto, UserId, cts.Token); // // This should give concurrency checking
            if (result.IsFailure)
            {
                return ValidationErrors(result);
            }

            return NoContent();
        }
        #endregion

        #region Bulk Delete
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Delete, ResourceCollectionsCore.CRUD.Operations.DeleteOwner)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Delete)]
        [Route("bulk")]
        [HttpDelete]
        public virtual async Task<ActionResult<List<ValidationProblemDetails>>> BulkDelete([FromBody] TDeleteDto[] dtos)
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            var results = await Service.BulkDeleteAsync(dtos, UserId, cts.Token);

            return BulkDeleteResponse(results);
        }
        #endregion

        #region Create New Child Collection Item Instance
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create, ResourceCollectionsCore.CRUD.Operations.Update)]
        [Route("new/{*collection}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ApiScopes.Create)]
        [HttpGet]
        public virtual IActionResult NewCollectionItem(string collection)
        {
            if (!RelationshipHelper.IsValidCollectionItemCreateExpression(collection, typeof(TUpdateDto)))
            {
                return BadRequest(Messages.CollectionInvalid);
            }

            var response = Service.GetCreateDefaultCollectionItemDto(collection);

            return Ok(response);
        }
        #endregion
    }
}

