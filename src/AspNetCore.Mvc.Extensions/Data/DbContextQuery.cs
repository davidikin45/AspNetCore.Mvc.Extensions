using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data
{
    public class DbContextQuery : DbContext
    {
        public virtual CountList<TEntity> Query<TEntity>(
         IncludeSpecification<TEntity> includeSpecification,
         FilterSpecification<TEntity> filterSpecification,
         OrderBySpecification<TEntity> orderBySpecification,
         int? skip = null,
         int? take = null) where TEntity : class
        {
            return GetQueryable<TEntity>(true, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), null, null, includeSpecification.ToExpression()).ToCountList(skip, take);
        }

        public virtual CountList<TEntity> QueryNoTracking<TEntity>(
         IncludeSpecification<TEntity> includeSpecification,
         FilterSpecification<TEntity> filterSpecification,
         OrderBySpecification<TEntity> orderBySpecification,
         int? skip = null,
         int? take = null) where TEntity : class
        {
            return GetQueryable<TEntity>(false, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), null, null, includeSpecification.ToExpression()).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> QueryAsync<TEntity>(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
        {
            return GetQueryable<TEntity>(true, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), null, null, includeSpecification.ToExpression()).ToCountListAsync(skip, take);
        }

        public virtual Task<CountList<TEntity>> QueryNoTrackingAsync<TEntity>(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
        {
            return GetQueryable<TEntity>(false, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), null, null, includeSpecification.ToExpression()).ToCountListAsync(skip, take);
        }

        private IQueryable<TEntity> GetQueryable<TEntity>(
            bool tracking,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            IQueryable<TEntity> query = this.Set<TEntity>();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            else
            {
                //By default tracking is QueryTrackingBehavior.TrackAll. If the DbContext is set to QueryTrackingBehavior.NoTracking we don't want to allow a user to override this behaviour.
                //query = query.AsTracking();
            }

            //include
            if (includeProperties != null && includeProperties.Count() > 0)
            {
                foreach (var includeExpression in includeProperties)
                {
                    query = query.Include(includeExpression);
                }
            }

            //where clause
            if (filter != null)
                query = query.Where(filter);

            //order by
            if (orderBy != null)
                query = orderBy(query);

            //skip
            if (skip.HasValue)
                query = query.Skip(skip.Value);

            //take
            if (take.HasValue)
                query = query.Take(take.Value);

            DebugSQL(query);

            return query;
        }

        private void DebugSQL<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            var sql = query.ToString();
        }
    }
}
