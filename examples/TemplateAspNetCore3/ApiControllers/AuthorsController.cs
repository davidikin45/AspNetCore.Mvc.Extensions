using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Controllers.Api;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Reflection;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TemplateAspNetCore3.ApplicationServices;
using TemplateAspNetCore3.Dtos;

namespace TemplateAspNetCore3.ApiControllers
{
    [ResourceCollection(ResourceCollections.Blog.Authors.CollectionId)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/blog/authors")]
    public class AuthorsController : ApiControllerEntityAuthorizeBase<AuthorDto, AuthorDto, AuthorDto, AuthorDeleteDto, IAuthorApplicationService>
    {
        public AuthorsController(IAuthorApplicationService service, IMapper mapper, IEmailService emailService, LinkGenerator linkGenerator, ITypeHelperService typeHelperService, AppSettings appSettings)
            : base(service, mapper, emailService, linkGenerator, typeHelperService, appSettings)
        {

        }
    }
}
