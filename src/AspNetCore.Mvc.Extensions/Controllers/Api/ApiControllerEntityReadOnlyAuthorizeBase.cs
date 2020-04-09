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
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    //Resource naming - Should be noun = thing, not an action. Example. api/getauthors is bad. api/authors is good.
    //Folow this principle for predictability. Represent hierachy. api/authors/{author}/books
    //Pluralize vs not pluralize is OK. Stay consistent
    //Filters,Sorting,Ordering are not resources. Should not be in URL, use query string instead.

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
    public abstract class ApiControllerEntityReadOnlyAuthorizeBase<TDto, IEntityService> : ApiControllerBase, IApiControllerEntityReadOnly<TDto>
        where TDto : class
        where IEntityService : IApplicationServiceEntityReadOnly<TDto>
    {
        public IEntityService Service { get; private set; }

        public ApiControllerEntityReadOnlyAuthorizeBase(ControllerServicesContext context, IEntityService service)
        : base(context)
        {
            Service = service;
        }

        #region Search
        /// <summary>
        /// Gets the paged.
        /// </summary>
        /// <param name="resourceParameters">The resource parameters.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [FormatFilter]
        [Route("")]
        [Route(".{format}")]
        [HttpGet]
        [HttpHead]
        public virtual async Task<ActionResult<WebApiListResponseDto<TDto>>> Search([FromQuery] WebApiSearchQueryParamsDto<TDto> resourceParameters)
        {
            if (User.Claims.Where(c => c.Type == JwtClaimTypes.Scope && c.Value.EndsWith(ResourceCollectionsCore.CRUD.Operations.Read)).Count() == 0)
            {
                resourceParameters.UserId = UserId;
            }

            return await List(resourceParameters);
        }

        private async Task<ActionResult> List(WebApiSearchQueryParamsDto<TDto> resourceParameters)
        {

            if (string.IsNullOrEmpty(resourceParameters.OrderBy))
                resourceParameters.OrderBy = "Id";

            if (!UIHelper.ValidOrderByFor<TDto>(resourceParameters.OrderBy))
            {
                return BadRequest(Messages.OrderByInvalid);
            }

            if (!UIHelper.ValidFieldsFor<TDto>(resourceParameters.Fields))
            {
                return BadRequest(Messages.FieldsInvalid);
            }

            if (!UIHelper.ValidFilterFor<TDto>(HttpContext.Request.Query))
            {
                return BadRequest(Messages.FiltersInvalid);
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var dataTask = Service.SearchAsync(cts.Token, resourceParameters.UserId, resourceParameters.Search, UIHelper.GetFilter<TDto>(HttpContext.Request.Query), resourceParameters.OrderBy, resourceParameters.Page, resourceParameters.PageSize, false);

            await TaskHelper.WhenAllOrException(cts, dataTask);

            var data = dataTask.Result;

            var paginationMetadata = new PagingInfoDto
            {
                Page = resourceParameters.Page,
                PageSize = resourceParameters.PageSize,
                Records = data.TotalCount,
                PreviousPageLink = null,
                NextPageLink = null
            };

            if (paginationMetadata.HasPrevious)
            {
                paginationMetadata.PreviousPageLink = CreateResourceUri(resourceParameters, ResourceUriType.PreviousPage);
            }

            if (paginationMetadata.HasNext)
            {
                paginationMetadata.NextPageLink = CreateResourceUri(resourceParameters, ResourceUriType.NextPage);
            }

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata).Replace(Environment.NewLine, ""));

            var links = CreateLinksForCollections(resourceParameters,
              paginationMetadata.HasNext, paginationMetadata.HasPrevious);

            var shapedData = UIHelper.ShapeListData(data.ToList(), resourceParameters.Fields);

            var shapedDataWithLinks = shapedData.Select(dto =>
            {
                var dtoAsDictionary = dto as IDictionary<string, object>;
                var dtoLinks = CreateLinks(
                    dtoAsDictionary["Id"].ToString(), resourceParameters.Fields);

                dtoAsDictionary.Add("links", dtoLinks);

                return dtoAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedDataWithLinks,
                links = links
            };

            return Ok(linkedCollectionResource);
        }
        #endregion

        #region GetById with Composition Properties
        //http://jakeydocs.readthedocs.io/en/latest/mvc/models/formatting.html
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [FormatFilter]
        [Route("{id}"), Route("{id}.{format}")]
        //[Route("get/{id}"), Route("get/{id}.{format}")]
        [HttpGet]
        public virtual async Task<ActionResult<TDto>> GetById(string id, [FromQuery] WebApiParamsDto<TDto> parameters)
        {

            if (!UIHelper.ValidFieldsFor<TDto>(parameters.Fields))
            {
                return BadRequest(Messages.FieldsInvalid);
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            //By passing true we include the Composition properties which should be any child or join entities.
            var response = await Service.GetByIdAsync(id, cts.Token);

            if (response == null)
            {
                return NotFound();
            }

            var links = CreateLinks(id, parameters.Fields);

            var linkedResourceToReturn = response.ShapeData(parameters.Fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);

            // Success(shapedData);
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [FormatFilter]
        [Route("({ids})"), Route("({ids}).{format}")]
        [HttpGet]
        [DelimitedQueryString(',', '|')]
        public virtual async Task<ActionResult<List<TDto>>> BulkGetByIds(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var response = await Service.GetByIdsAsync(cts.Token, ids);

            var list = response.ToList();

            if (ids.Count() != list.Count())
            {
                return NotFound();
            }

            return Ok(list);
        }

        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [FormatFilter]
        [Route("full-graph/{id}"), Route("full-graph/{id}.{format}")]
        [HttpGet]
        public virtual async Task<ActionResult<TDto>> GetByIdFullGraph(string id, [FromQuery] WebApiParamsDto<TDto> parameters)
        {
            if (!UIHelper.ValidFieldsFor<TDto>(parameters.Fields))
            {
                return BadRequest(Messages.FieldsInvalid);
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            //By passing true we should get the full graph of Composition and Aggregation Properties
            var response = await Service.GetByIdAsync(id, cts.Token, true);

            if (response == null)
            {
                return NotFound();
            }

            var links = CreateLinks(id, parameters.Fields, true);

            var linkedResourceToReturn = response.ShapeData(parameters.Fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);

            // Success(shapedData);
        }
        #endregion

        #region Child Collection (Navigation OR Owned Collection. Owned collection will get loaded with entity) List and Details
        //Would never allow either to be updated through this controller.

        /// <summary>
        /// Gets the paged.
        /// Service/Collection/Resource
        /// </summary>
        /// <param name="resourceParameters">The resource parameters.</param>
        /// <returns></returns>
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [FormatFilter]
        [Route("{id}/{*collection}")]
        //[Route("{id}/{*collection}.{format}")]
        [HttpGet]
        [HttpHead]
        public virtual async Task<IActionResult> GetByIdChildCollection(string id, string collection, WebApiSearchQueryParamsDto resourceParameters)
        {

            if (string.IsNullOrEmpty(resourceParameters.OrderBy))
                resourceParameters.OrderBy = "Id";


            //order by
            if (!UIHelper.ValidOrderByFor<TDto>(resourceParameters.OrderBy))
            {
                return BadRequest(Messages.OrderByInvalid);
            }

            if (!RelationshipHelper.IsValidCollectionExpression(collection, typeof(TDto)))
            {
                return BadRequest(Messages.CollectionInvalid);
            }

            if (RelationshipHelper.IsCollectionExpressionCollectionItem(collection))
            {
                return await GetCollectionItem(id, collection, resourceParameters.Fields);
            }

            var collectionItemType = RelationshipHelper.GetCollectionExpressionType(collection, typeof(TDto));

            //fields
            if (!UIHelper.ValidFieldsFor(collectionItemType, resourceParameters.Fields))
            {
                return BadRequest(Messages.FieldsInvalid);
            }

            //filter
            if (!UIHelper.ValidFilterFor(collectionItemType, HttpContext.Request.Query))
            {
                return BadRequest(Messages.FiltersInvalid);
            }

            var filter = UIHelper.GetFilter(collectionItemType, HttpContext.Request.Query);

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var result = await Service.GetByIdWithPagedCollectionPropertyAsync(cts.Token, id, collection, resourceParameters.Search, filter, resourceParameters.OrderBy, resourceParameters.Page, resourceParameters.PageSize);

            IEnumerable<Object> list = ((IEnumerable<Object>)RelationshipHelper.GetCollectionExpressionData(collection, typeof(TDto), result.Dto));

            var paginationMetadata = new PagingInfoDto
            {
                Page = resourceParameters.Page,
                PageSize = resourceParameters.PageSize,
                Records = result.TotalCount,
                PreviousPageLink = null,
                NextPageLink = null
            };

            if (paginationMetadata.HasPrevious)
            {
                paginationMetadata.PreviousPageLink = CreateCollectionPropertyResourceUri(collection, resourceParameters, ResourceUriType.PreviousPage);
            }

            if (paginationMetadata.HasNext)
            {
                paginationMetadata.NextPageLink = CreateCollectionPropertyResourceUri(collection, resourceParameters, ResourceUriType.NextPage);
            }

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata).Replace(Environment.NewLine, ""));

            var links = CreateLinksForCollectionProperty(collection, resourceParameters, paginationMetadata.HasNext, paginationMetadata.HasPrevious);

            var shapedData = UIHelper.ShapeListData(list, collectionItemType, resourceParameters.Fields);

            var shapedDataWithLinks = shapedData.Select(collectionPropertyDtoItem =>
            {
                var collectionPropertyDtoItemAsDictionary = collectionPropertyDtoItem as IDictionary<string, object>;
                var collectionPropertyDtoItemLinks = CreateLinksForCollectionItem(id, collection + "/" + collectionPropertyDtoItemAsDictionary["Id"].ToString(), resourceParameters.Fields);

                collectionPropertyDtoItemAsDictionary.Add("links", collectionPropertyDtoItem);

                return collectionPropertyDtoItemAsDictionary;
            }).ToList();

            var linkedCollectionResource = new WebApiListResponseDto<IDictionary<string, object>>
            {
                Value = shapedDataWithLinks
                ,
                Links = links
            };

            return Ok(linkedCollectionResource);
        }

        private async Task<IActionResult> GetCollectionItem(string id, string collection, [FromQuery] string fields)
        {
            if (!RelationshipHelper.IsValidCollectionExpression(collection, typeof(TDto)))
            {
                return BadRequest(Messages.CollectionInvalid);
            }

            if (!RelationshipHelper.IsCollectionExpressionCollectionItem(collection))
            {
                return BadRequest(Messages.CollectionInvalid);
            }

            var collectionItemType = RelationshipHelper.GetCollectionExpressionType(collection, typeof(TDto));
            if (!UIHelper.ValidFieldsFor(collectionItemType, fields))
            {
                return BadRequest(Messages.FieldsInvalid);
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            var response = await Service.GetByIdWithPagedCollectionPropertyAsync(cts.Token, id, collection, "", null, null, null);

            var collectionItem = RelationshipHelper.GetCollectionExpressionData(collection, typeof(TDto), response.Dto);

            if (collectionItem == null)
            {
                return NotFound();
            }

            var links = CreateLinksForCollectionItem(id, collection, fields);

            var linkedResourceToReturn = collectionItem.ShapeData(collectionItemType, fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }
        #endregion

        #region HATEOAS - Hypermedia as the Engine of Application State
        private string CreateResourceUri(WebApiSearchQueryParamsDto<TDto> resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Request.GetEncodedUrl().Replace($"page={resourceParameters.Page}", $"page={resourceParameters.Page - 1}");
                case ResourceUriType.NextPage:
                    return Request.GetEncodedUrl().Replace($"page={resourceParameters.Page}", $"page={resourceParameters.Page + 1}");

                default:
                    //https://stackoverflow.com/questions/30755827/getting-absolute-urls-using-asp-net-core
                    return Request.GetEncodedUrl();
            }
        }

        private string CreateCollectionPropertyResourceUri(string collection, WebApiSearchQueryParamsDto resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Action(nameof(BulkGetByIds),
                          Url.ActionContext.RouteData.Values["controller"].ToString(),
                      new
                      {
                          collection = collection,
                          fields = resourceParameters.Fields,
                          page = resourceParameters.Page - 1,
                          pageSize = resourceParameters.PageSize
                      },
                      Url.ActionContext.HttpContext.Request.Scheme);
                case ResourceUriType.NextPage:
                    return Url.Action(nameof(BulkGetByIds),
                          Url.ActionContext.RouteData.Values["controller"].ToString(),
                      new
                      {
                          collection = collection,
                          fields = resourceParameters.Fields,
                          page = resourceParameters.Page + 1,
                          pageSize = resourceParameters.PageSize
                      },
                      Url.ActionContext.HttpContext.Request.Scheme);

                default:
                    return Url.Action(nameof(BulkGetByIds),
                    Url.ActionContext.RouteData.Values["controller"].ToString(),
                    new
                    {
                        collection = collection,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page,
                        pageSize = resourceParameters.PageSize
                    },
                      Url.ActionContext.HttpContext.Request.Scheme);
            }
        }

        protected IEnumerable<LinkDto> CreateLinksForCreate()
        {
            var links = new List<LinkDto>();

            links.Add(
           new LinkDto(Url.Action("Create", Url.ActionContext.RouteData.Values["controller"].ToString(), Url.ActionContext.HttpContext.Request.Scheme),
           "create",
           HttpMethod.Post.Method));

            return links;
        }

        protected IEnumerable<LinkDto> CreateLinks(string id, string fields, bool fullGraph = false)
        {
            var links = new List<LinkDto>();

            string action = nameof(GetById);
            if (fullGraph)
            {
                action = nameof(GetByIdFullGraph);
            }

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Action(action, Url.ActionContext.RouteData.Values["controller"].ToString(), new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
                  "self",
                  HttpMethod.Get.Method));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Action(action, Url.ActionContext.RouteData.Values["controller"].ToString(), new { id = id, fields = fields }, Url.ActionContext.HttpContext.Request.Scheme),
                  "self",
                  HttpMethod.Get.Method));
            }

            links.Add(
              new LinkDto(Url.Action("DeleteDto", Url.ActionContext.RouteData.Values["controller"].ToString(), new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
              "delete",
              HttpMethod.Delete.Method));

            links.Add(
                new LinkDto(Url.Action("Update", Url.ActionContext.RouteData.Values["controller"].ToString(),
                new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
                "update",
                 HttpMethod.Put.Method));

            links.Add(
                new LinkDto(Url.Action("UpdatePartial", Url.ActionContext.RouteData.Values["controller"].ToString(),
                new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
                "partially_update",
                new HttpMethod("PATCH").Method));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCollectionItem(string id, string collection, string fields)
        {
            var links = new List<LinkDto>();

            //Create links for Collection Item Get, Delete and Update. Not sure if we want to allow 

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Action(nameof(BulkGetByIds), Url.ActionContext.RouteData.Values["controller"].ToString(), new { id = id, collection = collection }, Url.ActionContext.HttpContext.Request.Scheme),
                  "self",
                  HttpMethod.Get.Method));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Action(nameof(BulkGetByIds), Url.ActionContext.RouteData.Values["controller"].ToString(), new { id = id, collection = collection, fields = fields }, Url.ActionContext.HttpContext.Request.Scheme),
                  "self",
                  HttpMethod.Get.Method));
            }

            //Create links for Collection Item Delete and Update. Not sure if we want to allow this.

            //links.Add(
            //  new LinkDto(Url.Action("Delete", Url.ActionContext.RouteData.Values["controller"].ToString(), new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
            //  "delete",
            //  "DELETE"));

            //links.Add(
            //    new LinkDto(Url.Action("Update", Url.ActionContext.RouteData.Values["controller"].ToString(),
            //    new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
            //    "update",
            //    "PUT"));

            //links.Add(
            //    new LinkDto(Url.Action("UpdatePartial", Url.ActionContext.RouteData.Values["controller"].ToString(),
            //    new { id = id }, Url.ActionContext.HttpContext.Request.Scheme),
            //    "partially_update",
            //    "PATCH"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCollections(WebApiSearchQueryParamsDto<TDto> resourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateResourceUri(resourceParameters,
               ResourceUriType.Current)
               , "self", HttpMethod.Get.Method));

            links.Add(
           new LinkDto(Url.Action("Create", Url.ActionContext.RouteData.Values["controller"].ToString(),
          null, Url.ActionContext.HttpContext.Request.Scheme),
           "add",
           HttpMethod.Post.Method));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateResourceUri(resourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", HttpMethod.Get.Method));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateResourceUri(resourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", HttpMethod.Get.Method));
            }

            return links;
        }

        private List<LinkDto> CreateLinksForCollectionProperty(string collection, WebApiSearchQueryParamsDto resourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateCollectionPropertyResourceUri(collection, resourceParameters,
               ResourceUriType.Current)
               , "self", HttpMethod.Get.Method));

            //Todo if want to allow Add to collection property
            //  links.Add(
            // new LinkDto(Url.Action("Create", Url.ActionContext.RouteData.Values["controller"].ToString(),
            //null, Url.ActionContext.HttpContext.Request.Scheme),
            // "add",
            // "POST"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCollectionPropertyResourceUri(collection, resourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", HttpMethod.Get.Method));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateCollectionPropertyResourceUri(collection, resourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", HttpMethod.Get.Method));
            }

            return links;
        }
        #endregion
    }
}

