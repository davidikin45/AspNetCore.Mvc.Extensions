using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Controllers.Mvc;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TemplateAspNetCore3.ApplicationServices;
using TemplateAspNetCore3.Dtos;

namespace TemplateAspNetCore3.Areas.Admin.Controllers.Authors
{
    [Area("Admin")]
    [ResourceCollection(ResourceCollections.Blog.Authors.CollectionId)]
    [Route("admin/blog/authors")]
    public class AuthorsController : MvcControllerEntityAuthorizeBase<AuthorDto, AuthorDto, AuthorDto, AuthorDeleteDto, IAuthorApplicationService>
    {
        public AuthorsController(IAuthorApplicationService service, IMapper mapper, IEmailService emailService, AppSettings appSettings)
             : base(true, service, mapper, emailService, appSettings)
        {
        }
    }
}
