using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Data.Repository;
using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using AspNetCore.Mvc.Extensions.OrderByMapping;
using AspNetCore.Mvc.Extensions.Specification;
using AspNetCore.Mvc.Extensions.UI;
using AspNetCore.Mvc.Extensions.Validation.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Application
{
    public abstract class ApplicationServiceEntityReadOnlyBase<TEntity, TDto, TUnitOfWork> : ApplicationServiceBase, IApplicationServiceEntityReadOnly<TDto>
          where TEntity : class
          where TDto : class
          where TUnitOfWork : IUnitOfWork
    {
        protected virtual TUnitOfWork UnitOfWork { get; }
        protected virtual IGenericRepository<TEntity> Repository => UnitOfWork.Repository<TEntity>();

        public ApplicationServiceEntityReadOnlyBase(ApplicationervicesContext context, TUnitOfWork unitOfWork)
           : base(context)
        {
            UnitOfWork = unitOfWork;
        }

        public virtual void AddIncludes(List<Expression<Func<TEntity, Object>>> includes)
        {

        }
        public virtual bool GetFullGraph => false;

        #region GetAll
        public virtual IEnumerable<TDto> GetAll(
        string orderBy = null,
        int? pageNo = null,
        int? pageSize = null,
        bool getFullGraph = false,
        params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read).Wait();

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);

            //var orderByConverted = MapOrderBy<TDto, TEntity>(orderBy);
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var entityList = Repository.GetAll(orderByConverted, pageNo, pageSize, GetFullGraph || getFullGraph, includesConverted);

            IEnumerable<TDto> dtoList = entityList.ToList().Select(Mapper.Map<TEntity, TDto>);

            return dtoList;
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync(
            CancellationToken cancellationToken,
            //Expression<Func<IQueryable<TDto>, IOrderedQueryable<TDto>>> orderBy = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read);

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);
            //var orderByConverted = MapOrderBy<TDto, TEntity>(orderBy);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var entityList = await Repository.GetAllAsync(cancellationToken, orderByConverted, pageNo, pageSize, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            IEnumerable<TDto> dtoList = entityList.ToList().Select(Mapper.Map<TEntity, TDto>);

            return dtoList;
        }
        #endregion

        #region Search
        public virtual PagedList<TDto> Search(
       string ownedBy = null,
       string search = "",
       Expression<Func<TDto, bool>> filter = null,
       string orderBy = null,
       int? pageNo = null,
       int? pageSize = null,
       bool getFullGraph = false,
       params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            var entityList = Repository.Search(ownedBy, search, filterConverted, orderByConverted, skip, pageSize, GetFullGraph || getFullGraph, includesConverted);

            var entities = entityList.ToList();

            foreach (var entity in entities)
            {
                AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            }

            var dtoList = new PagedList<TDto>(entities.Select(Mapper.Map<TEntity, TDto>).ToList(), entityList.TotalCount, pageNo ?? 1, pageSize ?? entityList.TotalCount);

            return dtoList;
        }

        public virtual async Task<PagedList<TDto>> SearchAsync(
            CancellationToken cancellationToken,
             string ownedBy = null,
             string search = "",
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            var entityList = await Repository.SearchAsync(cancellationToken, ownedBy, search, filterConverted, orderByConverted, skip, pageSize, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            var entities = entityList.ToList();

            foreach (var entity in entities)
            {
               await AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);
            }

            var dtoList = new PagedList<TDto>(entities.Select(Mapper.Map<TEntity, TDto>).ToList(), entityList.TotalCount, pageNo ?? 1, pageSize ?? entityList.TotalCount);

            return dtoList;
        }
        #endregion

        #region Search Specification
        public virtual PagedList<TDto> Search(
          IncludeSpecification<TDto> includeSpecification,
          FilterSpecification<TDto> filterSpecification,
          OrderBySpecification<TDto> orderBySpecification,
          int? pageNo = null,
          int? pageSize = null,
          bool getFullGraph = false)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var filterConverted = filterSpecification.Map<TEntity>(Mapper);
    
            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBySpecification.OrderByString))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            //Ideally OrderByMapper could also convert Expressions but it only handles strings. 
            var orderByConverted = string.IsNullOrEmpty(orderBySpecification.OrderByString) ? orderBySpecification.Map<TEntity>(Mapper) : orderBySpecification.Map<TEntity>(OrderByMapper);

            var includesConverted = includeSpecification.Map<TEntity>(Mapper);

            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            var entityList = Repository.Search(includesConverted, filterConverted, orderByConverted, skip, pageSize, GetFullGraph || getFullGraph);

            var entities = entityList.ToList();

            foreach (var entity in entities)
            {
                AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            }

            var dtoList = new PagedList<TDto>(entities.Select(Mapper.Map<TEntity, TDto>).ToList(), entityList.TotalCount, pageNo ?? 1, pageSize ?? entityList.TotalCount);

            return dtoList;
        }

        public virtual async Task<PagedList<TDto>> SearchAsync(
          IncludeSpecification<TDto> includeSpecification,
          FilterSpecification<TDto> filterSpecification,
          OrderBySpecification<TDto> orderBySpecification,
          int? pageNo = null,
          int? pageSize = null,
          bool getFullGraph = false,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = filterSpecification.Map<TEntity>(Mapper);

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBySpecification.OrderByString))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            //Ideally OrderByMapper could also convert Expressions but it only handles strings. 
            var orderByConverted = string.IsNullOrEmpty(orderBySpecification.OrderByString) ? orderBySpecification.Map<TEntity>(Mapper) : orderBySpecification.Map<TEntity>(OrderByMapper);

            var includesConverted = includeSpecification.Map<TEntity>(Mapper);

            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            var entityList = await Repository.SearchAsync(includesConverted, filterConverted, orderByConverted, skip, pageSize, GetFullGraph || getFullGraph, cancellationToken).ConfigureAwait(false);

            var entities = entityList.ToList();

            foreach (var entity in entities)
            {
                await AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);
            }

            var dtoList = new PagedList<TDto>(entities.Select(Mapper.Map<TEntity, TDto>).ToList(), entityList.TotalCount, pageNo ?? 1, pageSize ?? entityList.TotalCount);

            return dtoList;
        }
        #endregion

        #region Get
        public virtual IEnumerable<TDto> Get(
           Expression<Func<TDto, bool>> filter = null,
           string orderBy = null,
           int? pageNo = null,
           int? pageSize = null,
           bool getFullGraph = false,
           params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);
            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            var entityList = Repository.Get(filterConverted, orderByConverted, skip, pageSize, GetFullGraph || getFullGraph, includesConverted);

            var entities = entityList.ToList();

            foreach (var entity in entities)
            {
                AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            }

            IEnumerable<TDto> dtoList = entities.Select(Mapper.Map<TEntity, TDto>);

            return dtoList;
        }

        public virtual async Task<IEnumerable<TDto>> GetAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);
            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);

            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            var entityList = await Repository.GetAsync(cancellationToken, filterConverted, orderByConverted, skip, pageSize, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            var entities = entityList.ToList();

            foreach (var entity in entities)
            {
                await AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);
            }

            IEnumerable<TDto> dtoList = entityList.ToList().Select(Mapper.Map<TEntity, TDto>);

            return dtoList;
        }

        public virtual int GetCount(
        Expression<Func<TDto, bool>> filter = null)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            return Repository.GetCount(filterConverted);
        }

        public virtual async Task<int> GetCountAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            return await Repository.GetCountAsync(cancellationToken, filterConverted).ConfigureAwait(false);
        }

        #endregion

        #region GetOne
        public virtual TDto GetOne(
          Expression<Func<TDto, bool>> filter = null,
          bool getFullGraph = false,
          params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var bo = Repository.GetOne(filterConverted, GetFullGraph || getFullGraph, includesConverted);

            AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            return Mapper.Map<TDto>(bo);
        }

        public virtual async Task<TDto> GetOneAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var bo = await Repository.GetOneAsync(cancellationToken, filterConverted, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            await AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            return Mapper.Map<TDto>(bo);
        }
        #endregion

        #region GetFirst
        public virtual TDto GetFirst(
         Expression<Func<TDto, bool>> filter = null,
         string orderBy = null,
         bool getFullGraph = false,
         params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var bo = Repository.GetFirst(filterConverted, orderByConverted, GetFullGraph || getFullGraph, includesConverted);

            AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            return Mapper.Map<TDto>(bo);
        }

        public virtual async Task<TDto> GetFirstAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null,
            string orderBy = null,
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            if (!OrderByMapper.ValidOrderByFor<TDto, TEntity>(orderBy))
            {
                throw new BadRequestError(Messages.OrderByInvalid);
            }

            var orderByConverted = OrderByMapper.GetOrderByDelegate<TDto, TEntity>(orderBy);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var bo = await Repository.GetFirstAsync(cancellationToken, filterConverted, orderByConverted, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            await AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            return Mapper.Map<TDto>(bo);
        }
        #endregion

        #region GetById
        public virtual TDto GetById(object id,
           bool getFullGraph = false,
           params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var bo = Repository.GetById(id, GetFullGraph || getFullGraph, includesConverted);

            AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            return Mapper.Map<TDto>(bo);
        }

        public virtual async Task<TDto> GetByIdAsync(object id,
            CancellationToken cancellationToken = default(CancellationToken),
            bool getFullGraph = false,
            params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var bo = await Repository.GetByIdAsync(cancellationToken, id, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            await AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            return Mapper.Map<TDto>(bo);
        }
        #endregion

        #region GetByIdWithPagedCollectionProperty
        public virtual (TDto Dto, int TotalCount) GetByIdWithPagedCollectionProperty(object id,
           string collectionExpression,
           string search = "",
           LambdaExpression filter = null,
           string orderBy = null,
           int? pageNo = null,
           int? pageSize = null)
        {
            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            var collectionItemType = RelationshipHelper.GetCollectionExpressionType(collectionExpression, typeof(TDto));
            var collectionItemMappedType = Mapper.ConfigurationProvider.GetAllTypeMaps().First(m => m.DestinationType == collectionItemType).SourceType;
            var filterConverted = MapWhereClause(collectionItemType, collectionItemMappedType, filter);

            var bo = Repository.GetByIdWithPagedCollectionProperty(id, collectionExpression, search, filterConverted, orderBy, skip, pageSize);

            AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();

            return (Mapper.Map<TDto>(bo.Entity), bo.TotalCount);
        }

        public virtual async Task<(TDto Dto, int TotalCount)> GetByIdWithPagedCollectionPropertyAsync(CancellationToken cancellationToken,
            object id,
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? pageNo = null,
            int? pageSize = null)
        {
            int? skip = null;
            if (pageNo.HasValue && pageSize.HasValue)
            {
                skip = (pageNo.Value - 1) * pageSize.Value;
            }

            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var collectionItemType = RelationshipHelper.GetCollectionExpressionType(collectionExpression, typeof(TDto));
            var collectionItemMappedType = Mapper.ConfigurationProvider.GetAllTypeMaps().First(m => m.DestinationType == collectionItemType).SourceType;
            var filterConverted = MapWhereClause(collectionItemType, collectionItemMappedType, filter);

            var bo = await Repository.GetByIdWithPagedCollectionPropertyAsync(cancellationToken, id, collectionExpression, search, filterConverted, orderBy, skip, pageSize);

            await AuthorizeResourceOperationAsync(bo, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            return (Mapper.Map<TDto>(bo.Entity), bo.TotalCount);
        }
        #endregion

        #region GetByIds
        public virtual IEnumerable<TDto> GetByIds(IEnumerable<object> ids,
        bool getFullGraph = false,
        params Expression<Func<TDto, Object>>[] includeProperties)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var result = Repository.GetByIds(ids, GetFullGraph || getFullGraph, includesConverted);

            var entities = result.ToList();

            foreach (var entity in entities)
            {
                AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            }

            return Mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<IEnumerable<TDto>> GetByIdsAsync(CancellationToken cancellationToken,
         IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TDto, Object>>[] includeProperties)
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var includesConverted = MapIncludes<TDto, TEntity>(includeProperties);
            var list = includesConverted.ToList();
            AddIncludes(list);
            includesConverted = list.ToArray();

            var result = await Repository.GetByIdsAsync(cancellationToken, ids, GetFullGraph || getFullGraph, includesConverted).ConfigureAwait(false);

            var entities = result.ToList();

            foreach (var entity in entities)
            {
                await AuthorizeResourceOperationAsync(entity, ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);
            }

            return Mapper.Map<IEnumerable<TDto>>(entities);
        }
        #endregion

        #region Exists
        public virtual bool Exists(Expression<Func<TDto, bool>> filter = null)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            return Repository.Exists(filterConverted);
        }

        public virtual async Task<bool> ExistsAsync(
            CancellationToken cancellationToken,
            Expression<Func<TDto, bool>> filter = null
            )
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            var filterConverted = MapWhereClause<TDto, TEntity>(filter);

            return await Repository.ExistsAsync(cancellationToken, filterConverted).ConfigureAwait(false);
        }

        public virtual bool Exists(object id)
        {
            AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner).Wait();
            return Repository.Exists(id);
        }

        public virtual async Task<bool> ExistsAsync(
            CancellationToken cancellationToken,
            object id
            )
        {
            await AuthorizeResourceOperationAsync(ResourceCollectionsCore.CRUD.Operations.Read, ResourceCollectionsCore.CRUD.Operations.ReadOwner);

            return await Repository.ExistsAsync(cancellationToken, id).ConfigureAwait(false);
        }
        #endregion
    }
}
