using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Repository
{
    public interface IGenericReadOnlyRepository<TEntity>
         where TEntity : class
    {
        IReadOnlyList<TEntity> SQLQuery(string query, params object[] paramaters);
        Task<IReadOnlyList<TEntity>> SQLQueryAsync(string query, params object[] paramaters);

        IReadOnlyList<TEntity> GetAll(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties);

        Task<IReadOnlyList<TEntity>> GetAllAsync(
                CancellationToken cancellationToken,
                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                int? skip = null,
                int? take = null,
                bool getFullGraph = false,
                params Expression<Func<TEntity, Object>>[] includeProperties)
                ;

        IReadOnlyList<TEntity> GetAllNoTracking(
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties);

        Task<IReadOnlyList<TEntity>> GetAllNoTrackingAsync(
                 CancellationToken cancellationToken,
                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                int? skip = null,
                int? take = null,
                bool getFullGraph = false,
                params Expression<Func<TEntity, Object>>[] includeProperties)
                ;

        CountList<TEntity> Search(
           string ownedBy = null,
           string search = "",
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           int? skip = null,
           int? take = null,
           bool getFullGraph = false,
           params Expression<Func<TEntity, Object>>[] includeProperties)
           ;

        Task<CountList<TEntity>> SearchAsync(
            CancellationToken cancellationToken,
            string ownedBy = null,
            string search = "",
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        CountList<TEntity> SearchNoTracking(
         string ownedBy = null,
         string search = "",
       Expression<Func<TEntity, bool>> filter = null,
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
       int? skip = null,
       int? take = null,
        bool getFullGraph = false,
       params Expression<Func<TEntity, Object>>[] includeProperties)
       ;

        Task<CountList<TEntity>> SearchNoTrackingAsync(
            CancellationToken cancellationToken,
            string ownedBy = null,
            string search = "",
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        CountList<TEntity> Search(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false);

       Task<CountList<TEntity>> SearchAsync(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          CancellationToken cancellationToken = default(CancellationToken));

       CountList<TEntity> SearchNoTracking(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false);

        Task<CountList<TEntity>> SearchNoTrackingAsync(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          CancellationToken cancellationToken = default(CancellationToken));

        IReadOnlyList<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        Task<IReadOnlyList<TEntity>> GetAsync(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        IReadOnlyList<TEntity> GetNoTracking(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
          ;

        Task<IReadOnlyList<TEntity>> GetNoTrackingAsync(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        TEntity GetOne(
            Expression<Func<TEntity, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        Task<TEntity> GetOneAsync(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        TEntity GetOneNoTracking(
        Expression<Func<TEntity, bool>> filter = null,
        bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        ;

        Task<TEntity> GetOneNoTrackingAsync(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        TEntity GetFirst(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        Task<TEntity> GetFirstAsync(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        TEntity GetFirstNoTracking(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
          ;

        Task<TEntity> GetFirstNoTrackingAsync(
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        TEntity GetById(object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        Task<TEntity> GetByIdAsync(
            CancellationToken cancellationToken,
            object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;


        TEntity GetByIdNoTracking(
            object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
          ;

        Task<TEntity> GetByIdNoTrackingAsync(CancellationToken cancellationToken,
            object id,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        (TEntity Entity, int TotalCount) GetByIdWithPagedCollectionProperty(object id,
      string collectionExpression,
      string search = "",
      LambdaExpression filter = null,
      string orderBy = null,
      int? skip = null,
      int? take = null);

        Task<(TEntity Entity, int TotalCount)> GetByIdWithPagedCollectionPropertyAsync(CancellationToken cancellationToken,
            object id,
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? skip = null,
            int? take = null);

        IReadOnlyList<TEntity> GetByIds(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
           ;

        Task<IReadOnlyList<TEntity>> GetByIdsAsync(CancellationToken cancellationToken,
            IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        IReadOnlyList<TEntity> GetByIdsNoTracking(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
       ;

        Task<IReadOnlyList<TEntity>> GetByIdsNoTrackingAsync(CancellationToken cancellationToken,
            IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
            ;

        int GetCount(Expression<Func<TEntity, bool>> filter = null)
            ;

        Task<int> GetCountAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null)
            ;

        bool Exists(Expression<Func<TEntity, bool>> filter = null)
            ;

        Task<bool> ExistsAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null)
            ;

        bool ExistsNoTracking(Expression<Func<TEntity, bool>> filter = null)
          ;

        Task<bool> ExistsNoTrackingAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null)
            ;

        bool Exists(object id)
         ;

        Task<bool> ExistsAsync(CancellationToken cancellationToken, object id)
           ;

        bool ExistsNoTracking(object id)
        ;

        Task<bool> ExistsNoTrackingAsync(CancellationToken cancellationToken, object id)
           ;

        bool ExistsById(object id)
        ;

        Task<bool> ExistsByIdAsync(CancellationToken cancellationToken,
            object id)
            ;

        bool ExistsByIdNoTracking(object id)
          ;

        Task<bool> ExistsByIdNoTrackingAsync(CancellationToken cancellationToken,
            object id)
            ;

    }
}
