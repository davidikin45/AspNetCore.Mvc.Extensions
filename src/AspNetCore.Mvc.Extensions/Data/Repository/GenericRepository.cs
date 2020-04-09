using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Repository
{
    //One per Aggregate Root
    public class GenericRepository<TEntity> : GenericReadOnlyRepository<TEntity>, IGenericRepository<TEntity>
   where TEntity : class
    {
        public GenericRepository(DbContext context)
            : base(context)
        {
        }

        #region Upsert
        //https://docs.microsoft.com/en-us/ef/core/saving/disconnected-entities#saving-single-entities
        public virtual TEntity AddOrUpdate(TEntity entity, string addedOrUpdatedBy)
        {
            var updatedEntity = Update(entity, addedOrUpdatedBy);
            var auditableEntity = updatedEntity as IEntityAuditable;
            if (auditableEntity != null && context.IsEntityStateAdded(updatedEntity))
            {
                auditableEntity.CreatedOn = DateTime.UtcNow;
                auditableEntity.CreatedBy = addedOrUpdatedBy;
            }
            return updatedEntity;
        }
        #endregion

        #region Insert
        public virtual TEntity Add(TEntity entity, string addedBy)
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

            return context.AddEntity(entity);
        }
        #endregion

        #region Update
        public virtual TEntity Update(TEntity entity, string updatedBy)
        {
            var auditableEntity = entity as IEntityAuditable;
            if (auditableEntity != null)
            {
                auditableEntity.UpdatedOn = DateTime.UtcNow;
                auditableEntity.UpdatedBy = updatedBy;
            }

            if(context.IsEntityStateDetached(entity))
            {
                //In disconnected mode the entire graph will be added/updated. 
                return context.UpdateEntity(entity);
            }
            else
            {
                //In Connected mode only changed related data will be updated. If we call context.UpdateEntity it will update everything!
                return entity;
            }
        }
        #endregion

        #region Delete
        public virtual void Delete(object id, string deletedBy)
        {
            TEntity entity = GetById(id); // For concurrency purposes need to get latest version
            Delete(entity, deletedBy);
        }

        public virtual void Delete(TEntity entity, string deletedBy)
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
                context.RemoveEntity(entity);
            }
        }

        public void SoftDelete(IEntitySoftDelete entity, string deletedBy)
        {
            entity.IsDeleted = true;
            entity.DeletedBy = deletedBy;
            entity.DeletedOn = DateTime.UtcNow;
            context.UpdateEntity((TEntity)entity);
        }
        #endregion

        #region Save Changes
        public virtual Task<bool> SaveAsync()
        {
            return SaveAsync(CancellationToken.None);
        }

        public virtual async Task<bool> SaveAsync(CancellationToken cancellationToken)
        {
            return (await context.SaveChangesAsync(cancellationToken) >= 0);
        }
        #endregion
    }
}
