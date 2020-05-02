using AspNetCore.Mvc.Extensions.Alerts;
using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Controllers.ApiClient;
using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.UI;
using AspNetCore.Specification.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
    public abstract class MvcControllerEntityClientAuthorizeBase<TCreateDto, TReadDto, TUpdateDto, TDeleteDto, IEntityService> : MvcControllerEntityReadOnlyClientAuthorizeBase<TReadDto, IEntityService>
        where TCreateDto : class
        where TReadDto : class
        where TUpdateDto : class
        where TDeleteDto : class
        where IEntityService : IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>
    {
        public MvcControllerEntityClientAuthorizeBase(ControllerServicesContext context, Boolean admin, IEntityService service)
        : base(context, admin, service)
        {
        }

        #region New Instance
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create)]
        //[Authorize(Policy = ApiScopes.Create)]
        [Route("new")]
        public async virtual Task<ActionResult> Create()
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            var instance = await Service.NewDefaultAsync(cts.Token);
            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
            return View("Create", instance);
        }
        #endregion

        #region Create
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create)]
        //[Authorize(Policy = ApiScopes.Create)]
        [HttpPost]
        [Route("new")]
        public virtual async Task<ActionResult> Create(TCreateDto dto)
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            if (ModelState.IsValid)
            {
                try
                {
                    await Service.CreateAsync(dto);
                    return RedirectToControllerDefault().WithSuccess(this, Messages.AddSuccessful);
                }
                catch (Exception ex)
                {
                    HandleUpdateException(ex);
                }
            }
            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
            //error
            return View("Create", dto);
        }
        #endregion

        #region Get for Edit
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update)]
        //[Authorize(Policy = ApiScopes.Update)]
        [Route("edit/{id}")]
        public virtual async Task<ActionResult> Edit(string id)
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();
            TUpdateDto data = null;
            try
            {
                data = await Service.GetByIdForEditAsync(id, cts.Token);
                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
                return View("Edit", data);
            }
            catch
            {
                return HandleReadException();
            }
        }
        #endregion

        #region Update
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Update, ResourceCollectionsCore.CRUD.Operations.UpdateOwner)]
        //[Authorize(Policy = ApiScopes.Update)]
        [HttpPost]
        [Route("edit/{id}")]
        public virtual async Task<ActionResult> Edit(string id, TUpdateDto dto)
        {
            //dto.Id = id;
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            if (ModelState.IsValid)
            {
                try
                {
                    await Service.UpdateAsync(id, dto);
                    return RedirectToControllerDefault().WithSuccess(this, Messages.UpdateSuccessful);
                }
                catch (Exception ex)
                {
                    HandleUpdateException(ex);
                }
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
            return View("Edit", dto);
        }
        #endregion

        #region Get for Delete
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Delete, ResourceCollectionsCore.CRUD.Operations.DeleteOwner)]
        //[Authorize(Policy = ApiScopes.Delete)]
        [Route("delete/{id}")]
        public virtual async Task<ActionResult> Delete(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            TDeleteDto data = null;
            try
            {
                data = await Service.GetByIdForDeleteAsync(id, cts.Token);
                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
                return View("Delete", data);
            }
            catch
            {
                return HandleReadException();
            }
        }
        #endregion

        #region Delete
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Delete, ResourceCollectionsCore.CRUD.Operations.DeleteOwner)]
        //[Authorize(Policy = ApiScopes.Delete)]
        [HttpPost, ActionName("Delete"), Route("delete/{id}")]
        public virtual async Task<ActionResult> DeleteConfirmed(string id, TDeleteDto dto)
        {
            var cts = TaskHelper.CreateNewCancellationTokenSource();

            if (ModelState.IsValid)
            {
                try
                {
                    await Service.DeleteAsync(id, dto);
                    return RedirectToControllerDefault().WithSuccess(this, Messages.DeleteSuccessful);
                }
                catch (Exception ex)
                {
                    HandleUpdateException(ex);
                }
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
            var data = await Service.GetByIdAsync(id, new WebApiParamsDto(), cts.Token);
            return View("Delete", data);
        }
        #endregion

        #region Create New Collection Item Instance
        [ResourceAuthorize(ResourceCollectionsCore.CRUD.Operations.Create, ResourceCollectionsCore.CRUD.Operations.Update)]
        //[Authorize(Policy = ApiScopes.Write)]
        [HttpGet]
        [Route("new/{*collection}")]
        public virtual async Task<ActionResult> CreateCollectionItem(string collection)
        {
            if (!RelationshipHelper.IsValidCollectionItemCreateExpression(collection, typeof(TUpdateDto)))
            {
                return HandleReadException();
            }

            var cts = TaskHelper.CreateNewCancellationTokenSource();

            ViewBag.Collection = collection.Replace("/", ".");
            ViewBag.CollectionIndex = Guid.NewGuid().ToString();

            var instance = await Service.NewCollectionItemAsync<dynamic>(collection, cts.Token);

            return PartialView("_CreateCollectionItem", instance);
        }
        #endregion

        #region Ajax Remote Validation
        //[Remote(action: "CustomFieldValidation", controller: "Home")]
        //public IActionResult CustomFieldValidation(string fieldValue)
        //{
        //    if (fieldValue == "007")
        //        return Json(data: "007 is already assigned to James Bond!");

        //    return Json(data: true);
        //}
        #endregion
    }
}

