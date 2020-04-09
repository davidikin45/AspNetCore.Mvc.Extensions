using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Mapping;
using AspNetCore.Mvc.Extensions.OrderByMapping;
using AspNetCore.Mvc.Extensions.Users;
using AspNetCore.Mvc.Extensions.Validation;
using AspNetCore.Mvc.Extensions.Validation.Errors;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Application
{
    public abstract class ApplicationServiceBase : IApplicationService
    {
        public IMapper Mapper { get; }
        public IAuthorizationService AuthorizationService { get; } //Transient
        public IUserService UserService { get; }
        public IValidationService ValidationService { get; }
        public IOrderByMapper OrderByMapper { get; }

        public ApplicationServiceBase(ApplicationervicesContext context)
        {
            Mapper = context.Mapper;
            AuthorizationService = context.AuthorizationService;
            UserService = context.UserService;
            ValidationService = context.ValidationService;
            OrderByMapper = context.OrderByMapper;
        }

        public async Task AuthorizeResourceOperationAsync(params string[] operations)
        {
            string collectionId = null;

            var resourceAttribute = (ResourceCollectionAttribute)this.GetType().GetCustomAttributes(typeof(ResourceCollectionAttribute), true).FirstOrDefault();
            if (resourceAttribute != null)
            {
                collectionId = resourceAttribute.CollectionId;
            }

            bool success = false;
            foreach (var operation in operations)
            {
                var operationName = operation;
                if (!string.IsNullOrWhiteSpace(collectionId))
                {
                    operationName = operationName + "," + collectionId + "." + operationName;
                }

                var authorizationResult = await AuthorizationService.AuthorizeAsync(UserService.User, operationName);
                if (authorizationResult.Succeeded)
                {
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                throw new UnauthorizedErrors(new GeneralError(String.Format(Messages.UnauthorisedServiceOperation, resourceAttribute.CollectionId + "." + operations.FirstOrDefault())));
            }
        }

        public async Task AuthorizeResourceOperationAsync(object resource, params string[] operations)
        {
            if (resource != null)
            {
                string collectionId = null;

                var resourceAttribute = (ResourceCollectionAttribute)this.GetType().GetCustomAttributes(typeof(ResourceCollectionAttribute), true).FirstOrDefault();
                if (resourceAttribute != null)
                {
                    collectionId = resourceAttribute.CollectionId;
                }

                bool success = true;
                foreach (var operation in operations)
                {
                    var operationName = operation;
                    if (!string.IsNullOrWhiteSpace(collectionId))
                    {
                        operationName = collectionId + "." + operationName;
                    }

                    var authorizationResult = await AuthorizationService.AuthorizeAsync(UserService.User, resource, new OperationAuthorizationRequirement() { Name = operationName });
                    if (!authorizationResult.Succeeded && authorizationResult.Failure.FailCalled)
                    {
                        success = false;
                    }
                    else if (authorizationResult.Succeeded)
                    {
                        success = true;
                    }
                }

                if (!success)
                {
                    throw new UnauthorizedErrors(new GeneralError(String.Format(Messages.UnauthorisedServiceOperation, resourceAttribute.CollectionId + "." + operations.FirstOrDefault())));
                }
            }
        }

        public Expression<Func<TDestination, Object>>[] MapIncludes<TSource, TDestination>(Expression<Func<TSource, Object>>[] includes)
        {
            return Mapper.MapIncludes<TSource, TDestination>(includes);
        }

        public Expression<Func<TDestination, bool>> MapWhereClause<TSource, TDestination>(Expression<Func<TSource, bool>> selector)
        {
            return Mapper.MapWhereClause<TSource, TDestination>(selector);
        }

        public LambdaExpression MapWhereClause(Type source, Type destination, LambdaExpression selector)
        {
            return Mapper.MapWhereClause(source, destination, selector);
        }

        public Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>> MapOrderBy<TSource, TDestination>(Expression<Func<IQueryable<TSource>, IOrderedQueryable<TSource>>> orderBy)
        {
            if (orderBy == null)
                return null;

            return Mapper.MapOrderBy<TSource, TDestination>(orderBy).Compile();
        }
    }
}
