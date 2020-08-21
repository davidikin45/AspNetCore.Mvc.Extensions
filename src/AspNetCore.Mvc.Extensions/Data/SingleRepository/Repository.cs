using AspNetCore.Mvc.Extensions.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.SingleRepository
{
    //services.AddScoped<IRepository, ApplicationRepository>();
    public abstract class Repository : ReadOnlyRepository, IRepository
    {
        public Repository(DbContext context)
            : base(context)
        {
        }

        #region Upsert
        //https://docs.microsoft.com/en-us/ef/core/saving/disconnected-entities#saving-single-entities
        public virtual TEntity AddOrUpdate<TEntity>(TEntity entity, string addedOrUpdatedBy) where TEntity : class
        {
            return (TEntity)AddOrUpdate(entity, addedOrUpdatedBy);
        }

        public virtual object AddOrUpdate(object entity, string addedOrUpdatedBy)
        {
            var updatedEntity = Update(entity, addedOrUpdatedBy);
            var auditableEntity = updatedEntity as IEntityAuditable;
            if (auditableEntity != null && context.Entry(entity).State == EntityState.Added)
            {
                auditableEntity.CreatedOn = DateTime.UtcNow;
                auditableEntity.CreatedBy = addedOrUpdatedBy;
            }
            return updatedEntity;
        }
        #endregion

        #region Insert
        public virtual TEntity Add<TEntity>(TEntity entity, string addedBy) where TEntity : class
        {
            return (TEntity)Add(entity, addedBy);
        }

        public virtual object Add(object entity, string addedBy)
        {
            var auditableEntity = entity as IEntityAuditable;
            if (auditableEntity != null)
            {
                auditableEntity.CreatedOn = DateTime.UtcNow;
                auditableEntity.CreatedBy = addedBy;
                auditableEntity.UpdatedOn = DateTime.UtcNow;
                auditableEntity.UpdatedBy = addedBy;
            }

            var ownedEntity = entity as IEntityOwned;
            if (ownedEntity != null)
            {
                ownedEntity.OwnedBy = addedBy;
            }

            return context.Add(entity).Entity;
        }
        #endregion

        #region Update
        public virtual TEntity Update<TEntity>(TEntity entity, string updatedBy) where TEntity : class
        {
            return (TEntity)Update(entity, updatedBy);
        }

        public virtual object Update(object entity, string updatedBy)
        {
            var auditableEntity = entity as IEntityAuditable;
            if (auditableEntity != null)
            {
                auditableEntity.UpdatedOn = DateTime.UtcNow;
                auditableEntity.UpdatedBy = updatedBy;
            }

            if (context.Entry(entity).State == EntityState.Detached)
            {
                //In disconnected mode the entire graph will be added/updated. 
                return context.Update(entity).Entity;
            }
            else
            {
                //In Connected mode only changed related data will be updated. If we call context.UpdateEntity it will update everything!
                return entity;
            }
        }
        #endregion

        #region Delete
        public virtual void Delete<TEntity>(object id, string deletedBy) where TEntity : class
        {
            TEntity entity = GetById<TEntity>(id); // For concurrency purposes need to get latest version
            Delete(entity, deletedBy);
        }

        public virtual void Delete(object entity, string deletedBy)
        {
            if(entity is IEntitySoftDelete)
            {
                var softDeleteEntity = entity as IEntitySoftDelete;
                SoftDelete(softDeleteEntity, deletedBy);
            }
            else
            {
                var auditableEntity = entity as IEntityAuditable;
                if (auditableEntity != null)
                {
                    auditableEntity.UpdatedOn = DateTime.UtcNow;
                    auditableEntity.UpdatedBy = deletedBy;
                }
                context.Remove(entity);
            }
        }

        public void SoftDelete(IEntitySoftDelete entity, string deletedBy)
        {
            entity.IsDeleted = true;
            entity.DeletedBy = deletedBy;
            entity.DeletedOn = DateTime.UtcNow;
            context.Update(entity);
        }
        #endregion

        #region Save Changes
        //If just using Repository require SaveChangesAsync, otherwise if also using UoW to a. wrap over multiple dbcontexts, b.Allow chained DbContext.Savechanges, only the first UoW completion will trigger DbContext.SaveChanges
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return context.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}
