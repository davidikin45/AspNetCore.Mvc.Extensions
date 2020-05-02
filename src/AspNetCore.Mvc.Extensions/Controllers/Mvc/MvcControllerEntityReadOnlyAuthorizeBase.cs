using AspNetCore.Mvc.Extensions.Application;
using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.UI;
using AspNetCore.Specification.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.Mvc
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
    public abstract class MvcControllerEntityReadOnlyAuthorizeBase<TDto, IEntityService> : MvcControllerBase
        where TDto : class
        where IEntityService : IApplicationServiceEntityReadOnly<TDto>
    {
        public IEntityService Service { get; private set; }
        public Boolean Admin { get; set; }

        public MvcControllerEntityReadOnlyAuthorizeBase(ControllerServicesContext context, Boolean admin, IEntityService service)
        : base(context)
        {
            Admin = admin;
            Service = service;
        }

        #region Search
        // GET: Default
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        //[Authorize(Policy = ApiScopes.Read)]
        [Route("")]
        public virtual async Task<ActionResult> Index(int p = 1, int pageSize = 10, string orderBy = "Id desc", string search = "")
        {
            var spec = UserFilterSpecification.Create<TDto>(HttpContext.Request.Query);
            if (!spec.IsValid)
            {
                return HandleReadException();
            }

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            try
            {
                var dataTask = Service.SearchAsync(cts.Token, null, search, spec.ToExpression(), orderBy, p, pageSize, false, null);

                await TaskHelper.WhenAllOrException(cts, dataTask);

                var data = dataTask.Result;

                var response = new WebApiPagedResponseDto<TDto>
                {
                    Page = p,
                    PageSize = pageSize,
                    Records = data.TotalCount,
                    Rows = data.ToList(),
                    OrderBy = orderBy,
                    Search = search
                };

                ViewBag.Search = search;
                ViewBag.Page = p;
                ViewBag.PageSize = pageSize;
                ViewBag.OrderBy = orderBy;

                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;

                return View("~/Views/Bootstrap4/List.cshtml", response);
                //return View("List", response);
            }
            catch
            {
                return HandleReadException();
            }
        }
        #endregion

        #region GetById
        // GET: Default/Details/5
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [Route("details/{id}")]
        public virtual async Task<ActionResult> Details(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            TDto data = null;
            try
            {
                data = await Service.GetByIdAsync(id, cts.Token, false);
                if (data == null)
                    return HandleReadException();
            }
            catch
            {
                return HandleReadException();
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;

            return View("~/Views/Bootstrap4/Details.cshtml", data);
            //return View("Details", data);
        }
        #endregion

        #region Child Collection List and Details
        //These should be non-owned collections. Navigation collections. e.g /author/1/books
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner)]
        [Route("details/{id}/{*collection}")]
        public virtual async Task<ActionResult> Collection(string id, string collection, int p = 1, int pageSize = 10, string orderBy = "Id desc", string search = "")
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
                Type collectionItemType = RelationshipHelper.GetCollectionExpressionType(collection, typeof(TDto));
                var result = await Service.GetByIdWithPagedCollectionPropertyAsync(cts.Token, id, collection, search, UIHelper.GetFilter(collectionItemType, HttpContext.Request.Query), orderBy, p, pageSize);

                object list = RelationshipHelper.GetCollectionExpressionData(collection, typeof(TDto), result.Dto);

                var webApiPagedResponseDtoType = typeof(WebApiPagedResponseDto<>).MakeGenericType(collectionItemType);
                var response = Activator.CreateInstance(webApiPagedResponseDtoType);

                response.SetPropValue(nameof(WebApiPagedResponseDto<Object>.Page), p);
                response.SetPropValue(nameof(WebApiPagedResponseDto<Object>.PageSize), pageSize);
                response.SetPropValue(nameof(WebApiPagedResponseDto<Object>.Records), result.TotalCount);
                response.SetPropValue(nameof(WebApiPagedResponseDto<Object>.Rows), list);
                response.SetPropValue(nameof(WebApiPagedResponseDto<Object>.OrderBy), orderBy);
                response.SetPropValue(nameof(WebApiPagedResponseDto<Object>.Search), search);

                ViewBag.Search = search;
                ViewBag.Page = p;
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

                return View("~/Views/Bootstrap4/List.cshtml", response);
                //return View("List", response);
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
                var response = await Service.GetByIdWithPagedCollectionPropertyAsync(cts.Token, id, collection, "", null, null, null);

                var collectionItem = RelationshipHelper.GetCollectionExpressionData(collection, typeof(TDto), response.Dto);

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

            return View("~/Views/Bootstrap4/Details.cshtml", data);
            //return View("Details", data);
        }
        #endregion
    }
}

