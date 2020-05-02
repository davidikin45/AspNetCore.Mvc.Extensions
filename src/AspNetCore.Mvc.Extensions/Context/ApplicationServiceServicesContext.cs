using AspNetCore.Mvc.Extensions.Users;
using AspNetCore.Mvc.Extensions.Validation;
using AspNetCore.Specification.OrderByMapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace AspNetCore.Mvc.Extensions.Context
{
    public class ApplicationervicesContext
    {
        public IWebHostEnvironment HostingEnvironment { get; }
        public IMapper Mapper { get; }
        public IAuthorizationService AuthorizationService { get; }
        public IUserService UserService { get; }
        public IValidationService ValidationService { get; }
        public IOrderByMapper OrderByMapper { get; }
        public ApplicationervicesContext(IWebHostEnvironment hostingEnvironment, IMapper mapper, IAuthorizationService authorizationService, IUserService userService, IValidationService validationService, IOrderByMapper orderByMapper)
        {
            HostingEnvironment = hostingEnvironment;
            Mapper = mapper;
            AuthorizationService = authorizationService;
            UserService = userService;
            ValidationService = validationService;
            OrderByMapper = orderByMapper;
        }
    }
}
