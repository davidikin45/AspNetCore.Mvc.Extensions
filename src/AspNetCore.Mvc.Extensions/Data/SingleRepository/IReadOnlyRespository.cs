using AspNetCore.Specification;
using AspNetCore.Specification.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.SingleRepository
{

    public interface IReadOnlyRepository
    {
        IReadOnlyList<TEntity> SQLQuery<TEntity>(string query, params object[] paramaters) where TEntity : class;
        IReadOnlyList<TEntity> SQLQuery<TEntity>(FormattableString query) where TEntity : class;
        Task<IReadOnlyList<TEntity>> SQLQueryAsync<TEntity>(string query, params object[] paramaters) where TEntity : class;
        Task<IReadOnlyList<TEntity>> SQLQueryAsync<TEntity>(FormattableString query) where TEntity : class;

        IReadOnlyList<TEntity> GetAll<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class;

        Task<IReadOnlyList<TEntity>> GetAllAsync<TEntity>(
                CancellationToken cancellationToken,
                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                int? skip = null,
                int? take = null,
                bool getFullGraph = false,
                params Expression<Func<TEntity, Object>>[] includeProperties)
                 where TEntity : class;

        IReadOnlyList<TEntity> GetAllNoTracking<TEntity>(
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class;

        Task<IReadOnlyList<TEntity>> GetAllNoTrackingAsync<TEntity>(
                 CancellationToken cancellationToken,
                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                int? skip = null,
                int? take = null,
                bool getFullGraph = false,
                params Expression<Func<TEntity, Object>>[] includeProperties)
                 where TEntity : class;

        CountList<TEntity> Search<TEntity>(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           int? skip = null,
           int? take = null,
           bool getFullGraph = false,
           params Expression<Func<TEntity, Object>>[] includeProperties)
           where TEntity : class;

        Task<CountList<TEntity>> SearchAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        CountList<TEntity> SearchNoTracking<TEntity>(
       Expression<Func<TEntity, bool>> filter = null,
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
       int? skip = null,
       int? take = null,
        bool getFullGraph = false,
       params Expression<Func<TEntity, Object>>[] includeProperties)
       where TEntity : class;

        Task<CountList<TEntity>> SearchNoTrackingAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        Expression<Func<TEntity, bool>> PredicateEntityById<TEntity>(object id);
        Expression<Func<TEntity, bool>> PredicateEntityById<TEntity>(IEnumerable<object> ids);
        Expression<Func<TEntity, bool>> PredicateEntityByStringContains<TEntity>(string values);
        Expression<Func<TEntity, bool>> And<TEntity>(Expression<Func<TEntity, bool>> leftExpression, Expression<Func<TEntity, bool>> rightExpression);
        Expression<Func<TEntity, bool>> Or<TEntity>(Expression<Func<TEntity, bool>> leftExpression, Expression<Func<TEntity, bool>> rightExpression);

        SpecificationDbQuery<TEntity> SpecificationQuery<TEntity>() where TEntity : class;

        SpecificationDbQuery<TEntity> SpecificationQuery<TEntity>(
         IncludeSpecification<TEntity> includeSpecification,
         FilterSpecification<TEntity> filterSpecification,
         OrderBySpecification<TEntity> orderBySpecification,
         int? skip = null,
         int? take = null,
         bool getFullGraph = false) where TEntity : class;


       IReadOnlyList<TEntity> Get<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        Task<IReadOnlyList<TEntity>> GetAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        IReadOnlyList<TEntity> GetNoTracking<TEntity>(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
          where TEntity : class;

        Task<IReadOnlyList<TEntity>> GetNoTrackingAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        TEntity GetOne<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        Task<TEntity> GetOneAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        TEntity GetOneNoTracking<TEntity>(
        Expression<Func<TEntity, bool>> filter = null,
        bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        where TEntity : class;

        Task<TEntity> GetOneNoTrackingAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        TEntity GetFirst<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        Task<TEntity> GetFirstAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        TEntity GetFirstNoTracking<TEntity>(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
          where TEntity : class;

        Task<TEntity> GetFirstNoTrackingAsync<TEntity>(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        TEntity GetById<TEntity>(object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        Task<TEntity> GetByIdAsync<TEntity>(
            CancellationToken cancellationToken,
            object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;


        TEntity GetByIdNoTracking<TEntity>(
            object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
          where TEntity : class;

        Task<TEntity> GetByIdNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        (TEntity Entity, int TotalCount) GetByIdWithPagedCollectionProperty<TEntity>(object id,
      string collectionExpression,
      string search = "",
      LambdaExpression filter = null,
      string orderBy = null,
      int? skip = null,
      int? take = null) where TEntity : class;

        Task<(TEntity Entity, int TotalCount)> GetByIdWithPagedCollectionPropertyAsync<TEntity>(CancellationToken cancellationToken,
            object id,
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? skip = null,
            int? take = null) where TEntity : class;

        IReadOnlyList<TEntity> GetByIds<TEntity>(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
           where TEntity : class;

        Task<IReadOnlyList<TEntity>> GetByIdsAsync<TEntity>(CancellationToken cancellationToken,
            IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        IReadOnlyList<TEntity> GetByIdsNoTracking<TEntity>(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
       where TEntity : class;

        Task<IReadOnlyList<TEntity>> GetByIdsNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
            where TEntity : class;

        int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class;

        Task<int> GetCountAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class;

        bool Exists<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class;

        Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class;

        bool ExistsNoTracking<TEntity>(Expression<Func<TEntity, bool>> filter = null)
          where TEntity : class;

        Task<bool> ExistsNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class;

        bool Exists<TEntity>(object id)
         where TEntity : class;

        Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken, object id)
           where TEntity : class;

        bool ExistsNoTracking<TEntity>(object id)
       where TEntity : class;

        Task<bool> ExistsNoTrackingAsync<TEntity>(CancellationToken cancellationToken, object id)
           where TEntity : class;

        bool ExistsById<TEntity>(object id) where TEntity : class;

        Task<bool> ExistsByIdAsync<TEntity>(CancellationToken cancellationToken, object id)
            where TEntity : class;

        bool ExistsByIdNoTracking<TEntity>(object id)
          where TEntity : class;

        Task<bool> ExistsByIdNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            object id)
            where TEntity : class;

    }
}
