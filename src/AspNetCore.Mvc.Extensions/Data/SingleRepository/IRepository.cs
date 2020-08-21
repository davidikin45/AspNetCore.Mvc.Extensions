using AspNetCore.Mvc.Extensions.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.SingleRepository
{
    public interface IRepository : IReadOnlyRepository
    {
        object AddOrUpdate(object entity, string addedOrUpdatedBy);
        TEntity AddOrUpdate<TEntity>(TEntity entity, string addedOrUpdatedBy) where TEntity : class;

        object Add(object entity, string addedBy);
        TEntity Add<TEntity>(TEntity entity, string addedBy) where TEntity : class;

        object Update(object entity, string updatedBy);
        TEntity Update<TEntity>(TEntity entity, string updatedBy) where TEntity : class;

        void Delete<TEntity>(object id, string deletedBy) where TEntity : class;
        void SoftDelete(IEntitySoftDelete entity, string deletedBy);
        void Delete(object entity, string deletedBy);

        //If just using Repository require SaveChangesAsync, otherwise if also using UoW to a. wrap over multiple dbcontexts, b.Allow chained DbContext.Savechanges, only the first UoW completion will trigger DbContext.SaveChanges
        //Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
