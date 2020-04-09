using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Controllers.ApiClient;
using AspNetCore.Mvc.Extensions.Controllers.Mvc;
using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.UI;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.MvcApiClient
{
    //Edit returns a view of the resource being edited, the Update updates the resource it self

    //C - Create - POST
    //R - Read - GET
    //U - Update - PUT
    //D - Delete - DELETE

    //If there is an attribute applied(via[HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
    //If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
    //Otherwise, the action supports the POST method.
    [Authorize()]
    //[Authorize(Policy = ApiScopes.Read)]
    public abstract class MvcControllerEntityReadOnlyClientAuthorizeBase<TDto, IEntityService> : MvcControllerBase
        where TDto : class
        where IEntityService : IApiControllerEntityReadOnlyClient<TDto>
    {
        public IEntityService Service { get; private set; }
        public Boolean Admin { get; private set; }

        public MvcControllerEntityReadOnlyClientAuthorizeBase(ControllerServicesContext context, Boolean admin, IEntityService service)
        : base(context)
        {
            Admin = admin;
            Service = service;
        }

        #region Search
        // GET: Default
        [Route("")]
        public virtual async Task<ActionResult> Index(int page = 1, int pageSize = 10, string orderBy = "Id desc", string search = "")
        {

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            try
            {
                var searchDto = new WebApiSearchQueryParamsDto()
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    OrderBy = orderBy
                };

                var resp = await Service.SearchAsync(searchDto, cts.Token);

                var response = new WebApiPagedResponseDto<TDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    Records = resp.pagingInfo.Records,
                    Rows = resp.data.Value.ToList(),
                    OrderBy = orderBy,
                    Search = search
                };

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.OrderBy = orderBy;

                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
                return View("List", response);
            }
            catch
            {
                return HandleReadException();
            }
        }
        #endregion

        #region GetById
        // GET: Default/Details/5
        [Route("details/{id}")]
        public virtual async Task<ActionResult> Details(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            TDto data = null;
            try
            {
                data = await Service.GetByIdAsync(id, new WebApiParamsDto(), cts.Token);
                if (data == null)
                    return HandleReadException();
            }
            catch
            {
                return HandleReadException();
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
            return View("Details", data);
        }
        #endregion

        #region Child Collection List and Details

        [Route("details/{id}/{*collection}")]
        public virtual async Task<ActionResult> Collection(string id, string collection, int page = 1, int pageSize = 10, string orderBy = "Id desc", string search = "")
        {
            if (!RelationshipHelper.IsValidCollectionExpression(collection, typeof(TDto)))
            {
                return HandleReadException();
            }

            if (RelationshipHelper.IsCollectionExpressionCollectionItem(collection))
            {
                return await CollectionItemDetails(id, collection);
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            try
            {

                var searchDto = new WebApiSearchQueryParamsDto()
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    OrderBy = orderBy
                };

                var resp = await Service.GetByIdChildCollectionAsync<JObject>(id, collection, searchDto, cts.Token);

                var response = new WebApiPagedResponseDto<JObject>
                {
                    Page = page,
                    PageSize = pageSize,
                    Records = resp.pagingInfo.Records,
                    Rows = resp.data.Value.ToList(),
                    OrderBy = orderBy,
                    Search = search
                };

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.OrderBy = orderBy;

                ViewBag.Collection = collection;
                ViewBag.Id = id;

                //For the time being collection properties are read only. DDD states that only the Aggregate Root should get updated.
                ViewBag.DisableCreate = true;
                ViewBag.DisableEdit = true;
                ViewBag.DisableDelete = true;
                ViewBag.DisableSorting = false;
                ViewBag.DisableEntityEvents = true;
                ViewBag.DisableSearch = false;

                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
                return View("List", response);
            }
            catch
            {
                return HandleReadException();
            }
        }

        private async Task<ActionResult> CollectionItemDetails(string id, string collection)
        {
            if (!RelationshipHelper.IsValidCollectionExpression(collection, typeof(TDto)))
            {
                return HandleReadException();
            }

            if (!RelationshipHelper.IsCollectionExpressionCollectionItem(collection))
            {
                return HandleReadException();
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            Object data = null;
            try
            {
                var collectionItem = await Service.GetByIdChildCollectionItemAsync<JObject>(id, collection.Substring(0, collection.LastIndexOf('\\')), collection.Substring(collection.LastIndexOf('\\')), cts.Token);

                if (collectionItem == null)
                {
                    return HandleReadException();
                }

                data = collectionItem;
            }
            catch
            {
                return HandleReadException();
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;

            ViewBag.DisableEdit = true;
            ViewBag.Collection = RelationshipHelper.GetCollectionExpressionWithoutCurrentCollectionItem(collection);
            ViewBag.Id = id;

            return View("Details", data);
        }
        #endregion
    }
}

