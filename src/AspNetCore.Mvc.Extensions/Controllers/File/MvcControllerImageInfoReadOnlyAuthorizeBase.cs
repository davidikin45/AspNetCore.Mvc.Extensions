using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Controllers.Mvc;
using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem;
using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.File;
using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.Mapping;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.UI;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
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
    [Authorize(Roles = "admin")]
    public abstract class MvcControllerImageInfoReadOnlyAuthorizeBase : MvcControllerBase
    {   
        public IFileSystemGenericRepositoryFactory FileSystemGenericRepositoryFactory { get; private set; }
        public Boolean Admin { get; set; }
        public Boolean IncludeSubDirectories { get; set; }
        public String PhysicalPath { get; set; }

        public MvcControllerImageInfoReadOnlyAuthorizeBase(ControllerServicesContext context, string physicalPath, Boolean includeSubDirectories, Boolean admin, IFileSystemGenericRepositoryFactory fileSystemGenericRepositoryFactory)
        : base(context)
        {
            PhysicalPath = physicalPath;
            IncludeSubDirectories = includeSubDirectories;
            Admin = admin;
            FileSystemGenericRepositoryFactory = fileSystemGenericRepositoryFactory;
        }

        // GET: Default
        [Route("")]
        public virtual async Task<ActionResult> Index(int p = 1, int pageSize = 10, string orderBy = nameof(FileInfo.LastWriteTime) + " desc", string search = "")
        {

            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
                  
            try
            {
                var repository = FileSystemGenericRepositoryFactory.CreateImageInfoRepositoryReadOnly(cts.Token, PhysicalPath, IncludeSubDirectories);

                var data = await repository.MetadataSearchAsync(search, null, UIHelper.GetOrderByIQueryableDelegate<FileInfo>(orderBy), (p - 1) * pageSize, pageSize);
                var total = await repository.GetSearchCountAsync(search, null);

                var rows = data.ToList().Select(Mapper.Map<ImageInfo, ImageInfoDto>).ToList();

                var response = new WebApiPagedResponseDto<ImageInfoDto>
                {
                    Page = p,
                    PageSize = pageSize,
                    Records = total,
                    Rows = rows,
                    OrderBy = orderBy,
                    Search = search

                };

                ViewBag.Search = search;
                ViewBag.Page = p;
                ViewBag.PageSize = pageSize;
                ViewBag.OrderBy = orderBy;

                ViewBag.DisableCreate = true;
                ViewBag.DisableSorting = true;
                ViewBag.DisableDelete = false;

                ViewBag.PageTitle = Title;
                ViewBag.Admin = Admin;
                //return View("List", response);
                return View("~/Views/Bootstrap4/List.cshtml", response);
            }
            catch
            {
                return HandleReadException();
            }
        }

        // GET: Default/Details/5
        [Route("details/{*id}")]
        public virtual async Task<ActionResult> Details(string id)
        {
            var cts = TaskHelper.CreateChildCancellationTokenSource(ClientDisconnectedToken());
            ImageInfo data = null;
            try
            {
                var repository = FileSystemGenericRepositoryFactory.CreateImageInfoRepositoryReadOnly(cts.Token, PhysicalPath, IncludeSubDirectories);

                data = await repository.MetadataGetByPathAsync(id.Replace("/","\\"));

                if (data == null)
                    return HandleReadException();
            }
            catch
            {
                return HandleReadException();
            }

            ViewBag.PageTitle = Title;
            ViewBag.Admin = Admin;

            var dto = Mapper.Map<ImageInfoDto>(data);

            //return View("Details", dto);
            return View("~/Views/Bootstrap4/Details.cshtml", dto);
        }  

    }
}

