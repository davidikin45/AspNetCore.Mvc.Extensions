using AspNetCore.Specification;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Application
{
    public interface IApplicationServiceEntityReadOnly<TDto> : IApplicationService
        where TDto : class
    {

        IEnumerable<TDto> GetAll(
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties);

        Task<IEnumerable<TDto>> GetAllAsync(
            CancellationToken cancellationToken,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        PagedList<TDto> Search(
         string ownedBy = null,
         string search = "",
         Expression<Func<TDto, bool>> filter = null,
         string orderBy = null,
         int? pageNo = null,
         int? pageSize = null,
         bool getFullGraph = false,
         params Expression<Func<TDto, Object>>[] includeProperties)
         ;

        Task<PagedList<TDto>> SearchAsync(
            CancellationToken cancellationToken,
            string ownedBy = null,
            string search = "",
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        PagedList<TDto> Search(
         IncludeSpecification<TDto> includeSpecification,
         FilterSpecification<TDto> filterSpecification,
         OrderBySpecification<TDto> orderBySpecification,
         int? pageNo = null,
         int? pageSize = null,
         bool getFullGraph = false);

        Task<PagedList<TDto>> SearchAsync(
         IncludeSpecification<TDto> includeSpecification,
         FilterSpecification<TDto> filterSpecification,
         OrderBySpecification<TDto> orderBySpecification,
         int? pageNo = null,
         int? pageSize = null,
         bool getFullGraph = false,
         CancellationToken cancellationToken = default(CancellationToken));

        IEnumerable<TDto> Get(
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        Task<IEnumerable<TDto>> GetAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        TDto GetOne(
            Expression<Func<TDto, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        Task<TDto> GetOneAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        TDto GetFirst(
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        Task<TDto> GetFirstAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
            ;

        TDto GetById(object id, 
            bool getFullGraph = false, 
            params Expression<Func<TDto, Object>>[] includeProperties);

        Task<TDto> GetByIdAsync(object id, 
             CancellationToken cancellationToken, 
             bool getFullGraph = false, 
             params Expression<Func<TDto, Object>>[] includeProperties);

        (TDto Dto, int TotalCount) GetByIdWithPagedCollectionProperty(object id, 
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? pageNo = null, 
            int? pageSize = null);

        Task<(TDto Dto, int TotalCount)> GetByIdWithPagedCollectionPropertyAsync(CancellationToken cancellationToken, 
            object id, 
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? pageNo = null, 
            int? pageSize = null);

        IEnumerable<TDto> GetByIds(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TDto, Object>>[] includeProperties);

        Task<IEnumerable<TDto>> GetByIdsAsync(CancellationToken cancellationToken,
         IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TDto, Object>>[] includeProperties);

        int GetCount(Expression<Func<TDto, bool>> filter = null);

        Task<int> GetCountAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null
            );

        bool Exists(Expression<Func<TDto, bool>> filter = null);

        Task<bool> ExistsAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null);

        bool Exists(object id);

        Task<bool> ExistsAsync(
            CancellationToken cancellationToken,
            object id);
    }
}
