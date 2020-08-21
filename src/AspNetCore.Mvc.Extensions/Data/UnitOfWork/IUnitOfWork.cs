using AspNetCore.Mvc.Extensions.Data.Repository;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        bool CommitingChanges { get; set; }

        bool AutoDetectChangesEnabled { get; set; }
        QueryTrackingBehavior QueryTrackingBehavior { get; set; }

        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

        //DbContext DbContextByEntityType<TEntity>();
        //DbContext DbContextByEntityType(Type entityType);

        Result<int> Complete();
        Task<Result<int>> CompleteAsync();
        Task<Result<int>> CompleteAsync(CancellationToken cancellationToken);
    }
}
