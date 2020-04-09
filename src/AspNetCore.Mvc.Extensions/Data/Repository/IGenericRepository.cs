using AspNetCore.Mvc.Extensions.Domain;

namespace AspNetCore.Mvc.Extensions.Data.Repository
{
    public interface IGenericRepository<TEntity> : IGenericReadOnlyRepository<TEntity>
      where TEntity : class
    {
        TEntity AddOrUpdate(TEntity entity, string addedOrUpdatedBy);     
        TEntity Add(TEntity entity, string addedBy);
        TEntity Update(TEntity entity, string updatedBy);
        void Delete(object id, string deletedBy);
        void SoftDelete(IEntitySoftDelete entity, string deletedBy);
        void Delete(TEntity entity, string deletedBy);
    }
}
