using AspNetCore.Mvc.Extensions.ApplicationServices;
using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.SignalR;
using AspNetCore.Mvc.Extensions.Users;
using AspNetCore.Mvc.Extensions.Validation;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data;
using TemplateAspNetCore3.Dtos;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.ApplicationServices
{
    public interface IAuthorApplicationService : IApplicationServiceEntity<AuthorDto, AuthorDto, AuthorDto, AuthorDeleteDto>
    {
        Task<AuthorDto> GetAuthorAsync(string authorSlug, CancellationToken cancellationToken);
    }

    [ResourceCollection(ResourceCollections.Blog.Authors.CollectionId)]
    public class AuthorApplicationService : ApplicationServiceEntityBase<Author, AuthorDto, AuthorDto, AuthorDto, AuthorDeleteDto, IAppUnitOfWork>, IAuthorApplicationService
    {
        public AuthorApplicationService(IAppUnitOfWork unitOfWork, IMapper mapper, IAuthorizationService authorizationService, IUserService userService, IValidationService validationService, IHubContext<ApiNotificationHub<AuthorDto>> hubContext)
        : base(unitOfWork, mapper, authorizationService, userService, validationService, hubContext)
        {

        }

        public async Task<AuthorDto> GetAuthorAsync(string authorSlug, CancellationToken cancellationToken)
        {
            var bo = await UnitOfWork.AuthorRepository.GetAuthorAsync(authorSlug, cancellationToken);
            return Mapper.Map<AuthorDto>(bo);
        }

    }
}
