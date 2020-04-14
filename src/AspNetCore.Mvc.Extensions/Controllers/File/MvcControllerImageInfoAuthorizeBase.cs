using AspNetCore.Mvc.Extensions.Alerts;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem;
using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.File;
using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.File
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
    public abstract class MvcControllerImageInfoAuthorizeBase : MvcControllerImageInfoReadOnlyAuthorizeBase
    {
        public MvcControllerImageInfoAuthorizeBase(ControllerServicesContext context, string physicalPath, Boolean includeSubDirectories, Boolean admin, IFileSystemGenericRepositoryFactory fileSystemGenericRepositoryFactory, IMapper mapper = null, IEmailService emailService = null)
        : base(context, physicalPath, includeSubDirectories, admin, fileSystemGenericRepositoryFactory)
        {
        }

        // GET: Default/Edit/5
        [Route("edit/{*id}")]
        public virtual async Task<ActionResult> Edit(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            ImageInfo data = null;
            try
            {
                var repository = FileSystemGenericRepositoryFactory.CreateImageInfoRepositoryReadOnly(cts.Token, PhysicalPath, IncludeSubDirectories);
                data = await repository.MetadataGetByPathAsync(id.Replace("/", "\\"));

                var dto = Mapper.Map<ImageInfoDto>(data);

                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
              //  return View("Edit", dto);
                return View("~/Views/Bootstrap4/Edit.cshtml", dto);
            }
            catch
            {
                return HandleReadException();
            }
        }

        // POST: Default/Edit/5
        [HttpPost]
        [Route("edit/{*id}")]
        public virtual ActionResult Edit(string id, ImageInfoDto dto)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            if (ModelState.IsValid)
            {
                try
                {
                    var metadata = new ImageInfo(PhysicalPath + id.Replace("/", "\\"));
                    Mapper.Map(dto, metadata);

                    metadata.SaveWithCaption(dto.Caption, dto.DateCreated);

                    //await Service.UpdateAsync(dto, cts.Token);
                    return RedirectToControllerDefault().WithSuccess(this, Messages.UpdateSuccessful);
                }
                catch (Exception ex)
                {
                    HandleUpdateException(ex);
                }
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
           // return View("Edit", dto);
            return View("~/Views/Bootstrap4/Edit.cshtml", dto);
        }

        // GET: Default/Delete/5
        [Route("delete/{*id}")]
        public virtual async Task<ActionResult> Delete(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            ImageInfo data = null;
            try
            {

                var repository = FileSystemGenericRepositoryFactory.CreateImageInfoRepositoryReadOnly(cts.Token, PhysicalPath, IncludeSubDirectories);
                data = await repository.MetadataGetByPathAsync(id.Replace("/", "\\"));

                var dto = Mapper.Map<ImageInfoDto>(data);

                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
             //   return View("Delete", dto);
                return View("~/Views/Bootstrap4/Delete.cshtml", dto);
            }
            catch
            {
                return HandleReadException();
            }
        }

        // POST: Default/Delete/5
        [HttpPost, ActionName("Delete"), Route("delete/{*id}")]
        public virtual ActionResult DeleteConfirmed(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());

            if (ModelState.IsValid)
            {
                try
                {
                    var repository = FileSystemGenericRepositoryFactory.CreateImageInfoRepository(cts.Token, PhysicalPath, IncludeSubDirectories);
                    repository.Delete(id.Replace("/", "\\"));

                    return RedirectToControllerDefault().WithSuccess(this, Messages.DeleteSuccessful);
                }
                catch (Exception ex)
                {
                    HandleUpdateException(ex);
                }
            }
            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;
           // return View("Delete", id);
            return View("~/Views/Bootstrap4/Delete.cshtml", id);
        }
    }
}

