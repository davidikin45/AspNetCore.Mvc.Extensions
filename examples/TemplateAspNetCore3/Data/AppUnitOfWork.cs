using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.DomainEvents;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using TemplateAspNetCore3.Data.Repositories;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data
{
    public interface IAppUnitOfWork : IUnitOfWork
    {
        IAuthorRepository AuthorRepository { get; }
    }

    public class AppUnitOfWork : UnitOfWorkWithEventsBase, IAppUnitOfWork
    {
        public IAuthorRepository AuthorRepository { get; private set; }

        public AppUnitOfWork(IValidationService validationService, IDomainEventBus domainEventBus, AppContext appContext)
            : base(true, validationService, domainEventBus, appContext)
        {

        }

        public override void InitializeRepositories(Dictionary<Type, DbContext> contextsByEntityType)
        {
            AuthorRepository = new AuthorRepository((AppContext)contextsByEntityType[typeof(Author)]);

            AddRepository(AuthorRepository);
        }
    }
}
