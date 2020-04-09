using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Domain;
using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Repository
{
    //https://cpratt.co/truly-generic-repository/
    //System.Linq.Dynamic.Core
    public class GenericReadOnlyRepository<TEntity> : IGenericReadOnlyRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbContext context;

        //AsNoTracking() causes EF to bypass cache for reads and writes - Ideal for Web Applications and short lived contexts
        public GenericReadOnlyRepository(DbContext context)
        {
            this.context = context;
        }

        //Can only pass Expression<Func> into EF as it can convert those to SQL
        //EF cant handle Func<> as they are compiled!
        protected virtual IQueryable<TEntity> GetQueryable(
            bool tracking,
            string ownedBy = null,
            string search = "",
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            //includeProperties = includeProperties ?? string.Empty;
            IQueryable<TEntity> query = context.Set<TEntity>();
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            else
            {
                //By default tracking is QueryTrackingBehavior.TrackAll. If the DbContext is set to QueryTrackingBehavior.NoTracking we don't want to allow a user to override this behaviour.
                //query = query.AsTracking();
            }

            //where clause
            if (filter != null)
                query = query.Where(filter);

            if (ownedBy != null && typeof(IEntityOwned).IsAssignableFrom(typeof(TEntity)))
                query = query.Where((e) => ((IEntityOwned)e).OwnedBy == ownedBy || ((IEntityOwned)e).OwnedBy == null);

            if (!string.IsNullOrEmpty(search))
                query = CreateSearchQuery(query, search);

            //include
            if (getFullGraph)
            {
                var includesList = GetFullGraphIncludes(typeof(TEntity));

                foreach (var include in includesList)
                {
                    query = query.Include(include);
                }
            }
            else
            {
                if (includeProperties != null && includeProperties.Count() > 0)
                {
                    foreach (var includeExpression in includeProperties)
                    {
                        query = query.Include(includeExpression);
                    }
                }
            }

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

        private List<string> GetFullGraphIncludes(Type type, int maxDepth = 10)
        {
            return GetAllCompositionAndAggregationRelationshipPropertyIncludes(false, type, null, 0, maxDepth);
        }

        // Association = Composition (Doesn't exist without parent) or Aggregation (Exists without parent)
        private List<string> GetAllCompositionAndAggregationRelationshipPropertyIncludes(bool compositionRelationshipsOnly, Type type, string path = null, int depth = 0, int maxDepth = 5)
        {
            List<string> includesList = new List<string>();
            if (depth > maxDepth)
            {
                return includesList;
            }

            List<Type> excludeTypes = new List<Type>()
            {
                typeof(DateTime),
                typeof(String),
                typeof(byte[])
           };

            IEnumerable<PropertyInfo> properties = type.GetProperties().Where(p =>
            //Ignore value types
            p.CanWrite && !p.PropertyType.IsValueType && !excludeTypes.Contains(p.PropertyType)
            &&
            (
                //One way traversal.
                (p.PropertyType.IsCollection() && type != p.PropertyType.GetGenericArguments().First())
                ||
                //Link to another Aggregate.
                (!p.PropertyType.IsCollection() && type != p.PropertyType && !compositionRelationshipsOnly)
                )
            ).ToList();

            foreach (var p in properties)
            {
                var includePath = !string.IsNullOrWhiteSpace(path) ? path + "." + p.Name : p.Name;

                includesList.Add(includePath);

                Type propType = null;
                if (p.PropertyType.IsCollection())
                {
                    propType = type.GetGenericArguments(p.Name).First();
                }
                else
                {
                    propType = p.PropertyType;
                }

                includesList.AddRange(GetAllCompositionAndAggregationRelationshipPropertyIncludes(compositionRelationshipsOnly, propType, includePath, depth + 1, maxDepth));
            }

            return includesList;
        }

        private static MethodInfo contains_method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        private IQueryable<T> CreateSearchQuery<T>(IQueryable<T> query, string values)
        {
            List<Expression> andExpressions = new List<Expression>();

            ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

            var ignore = new List<string>() { "" };

            var entityType = context.Model.FindEntityType(typeof(T));

            var properties = entityType.GetProperties().Where(p => p.ClrType == typeof(string) && !p.IsShadowProperty() && !ignore.Contains(p.Name));

            //.NET Core 2.2
            //var properties = entityType.GetProperties().Where(p => p.ClrType == typeof(string) && !p.IsShadowProperty && !ignore.Contains(p.Name));

            foreach (var value in values.Split('&'))
            {
                List<Expression> orExpressions = new List<Expression>();

                foreach (PropertyInfo prop in properties.Select(p => p.PropertyInfo))
                {

                    MemberExpression member_expression = Expression.PropertyOrField(parameter, prop.Name);

                    ConstantExpression value_expression = Expression.Constant(value, typeof(string));

                    MethodCallExpression contains_expression = Expression.Call(member_expression, contains_method, value_expression);

                    //var likeMethod = typeof(DbFunctionsExtensions)
                    //  .GetMethods()
                    //  .Where(p => p.Name == "Like")
                    //  .First();

                    //string pattern = $"%{value}%";
                    //ConstantExpression likeConstant = Expression.Constant(pattern, typeof(string));

                    //ConstantExpression dbFunctions_expression = Expression.Constant(EF.Functions, typeof(DbFunctions));
                    //ConstantExpression property_expression = Expression.Constant(prop.Name, typeof(string));
                    //MethodCallExpression like_expression = Expression.Call(method: likeMethod, arguments: new[] { dbFunctions_expression, property_expression, likeConstant });

                    orExpressions.Add(contains_expression);
                }

                if (orExpressions.Count == 0)
                    return query;

                Expression or_expression = orExpressions[0];

                for (int i = 1; i < orExpressions.Count; i++)
                {
                    or_expression = Expression.OrElse(or_expression, orExpressions[i]);
                }

                andExpressions.Add(or_expression);
            }

            if (andExpressions.Count == 0)
                return query;

            Expression and_expression = andExpressions[0];

            for (int i = 1; i < andExpressions.Count; i++)
            {
                and_expression = Expression.AndAlso(and_expression, andExpressions[i]);
            }

            Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(
                and_expression, parameter);

            return query.Where(expression);
        }

        private void DebugSQL(IQueryable<TEntity> query)
        {
            var sql = query.ToString();
        }

        #region SQLQuery
        public virtual IReadOnlyList<TEntity> SQLQuery(string query, params object[] paramaters)
        {

            return context.Set<TEntity>().FromSqlRaw(query, paramaters).AsNoTracking().ToList();

            //.NET Core 2.2 
            //return context.Set<TEntity>().AsNoTracking().FromSql(query, paramaters).ToList();
        }

        public async virtual Task<IReadOnlyList<TEntity>> SQLQueryAsync(string query, params object[] paramaters)
        {
            return await context.Set<TEntity>().FromSqlRaw(query, paramaters).AsNoTracking().ToListAsync();
            //.NET Core 2.2 
            //return await context.Set<TEntity>().AsNoTracking().FromSql(query, paramaters).ToListAsync();
        }
        #endregion

        #region GetAll
        public virtual IReadOnlyList<TEntity> GetAll(
         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         int? skip = null,
         int? take = null,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, null, null, null, orderBy, skip, take, getFullGraph, includeProperties).ToList().AsReadOnly();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return await GetQueryable(true, null, null, null, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual IReadOnlyList<TEntity> GetAllNoTracking(
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int? skip = null,
        int? take = null,
        bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, null, null, null, orderBy, skip, take, getFullGraph, includeProperties).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllNoTrackingAsync(CancellationToken cancellationToken,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return await GetQueryable(false, null, null, null, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Get
        public virtual IReadOnlyList<TEntity> Get(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, null, null, filter, orderBy, skip, take, getFullGraph, includeProperties).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return await GetQueryable(true, null, null, filter, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual IReadOnlyList<TEntity> GetNoTracking(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, null, null, filter, orderBy, skip, take, getFullGraph, includeProperties).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetNoTrackingAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return await GetQueryable(false, null, null, filter, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual int GetCount(Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(false, null, null, filter).Count();
        }

        public virtual Task<int> GetCountAsync(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(false, null, null, filter).CountAsync(cancellationToken);
        }
        #endregion

        #region Search
        public virtual CountList<TEntity> Search(
          string ownedBy = null,
          string search = "",
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
         bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, ownedBy, search, filter, orderBy, null, null, getFullGraph, includeProperties).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> SearchAsync(CancellationToken cancellationToken,
             string ownedBy = null,
             string search = "",
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
           bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, ownedBy, search, filter, orderBy, null, null, getFullGraph, includeProperties).ToCountListAsync(skip, take);
        }

        public virtual CountList<TEntity> SearchNoTracking(
          string ownedBy = null,
          string search = "",
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, ownedBy, search, filter, orderBy, null, null, getFullGraph, includeProperties).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> SearchNoTrackingAsync(CancellationToken cancellationToken,
            string ownedBy = null,
             string search = "",
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, ownedBy, search, filter, orderBy, null, null, getFullGraph, includeProperties)
                .ToCountListAsync(skip, take, cancellationToken);
        }
        #endregion

        #region Search Specification Pattern
        public virtual CountList<TEntity> Search(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false)
        {
            return GetQueryable(true, null, null, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), 
                null, null, getFullGraph, includeSpecification.ToExpression()).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> SearchAsync(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetQueryable(true, null, null, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), 
                null, null, getFullGraph, includeSpecification.ToExpression()).ToCountListAsync(skip, take);
        }

        public virtual CountList<TEntity> SearchNoTracking(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false)
        {
            return GetQueryable(false, null, null, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(), 
                null, null, getFullGraph, includeSpecification.ToExpression()).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> SearchNoTrackingAsync(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetQueryable(false, null, null, filterSpecification.ToExpression(), orderBySpecification.ToExpression().Compile(),
                null, null, getFullGraph, includeSpecification.ToExpression()).ToCountListAsync(skip, take);
        }
        #endregion

        #region GetOne
        public virtual TEntity GetOne(
         Expression<Func<TEntity, bool>> filter = null,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, null, null, filter, null, null, null, getFullGraph).SingleOrDefault();
        }

        public virtual Task<TEntity> GetOneAsync(CancellationToken cancellationToken,
          Expression<Func<TEntity, bool>> filter = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken);
        }

        public virtual TEntity GetOneNoTracking(
         Expression<Func<TEntity, bool>> filter = null,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefault();
        }

        public virtual Task<TEntity> GetOneNoTrackingAsync(CancellationToken cancellationToken,
          Expression<Func<TEntity, bool>> filter = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken);
        }
        #endregion

        #region GetFirst
        public virtual TEntity GetFirst(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, null, null, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefault();
        }

        public virtual Task<TEntity> GetFirstAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(true, null, null, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual TEntity GetFirstNoTracking(
         Expression<Func<TEntity, bool>> filter = null,
         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, null, null, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefault();
        }

        public virtual Task<TEntity> GetFirstNoTrackingAsync(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            return GetQueryable(false, null, null, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefaultAsync(cancellationToken);
        }
        #endregion

        #region GetById
        public virtual TEntity GetById(object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            if (getFullGraph || (includeProperties != null && includeProperties.Count() > 0))
            {
                Expression<Func<TEntity, bool>> filter = SearchForEntityById(id);
                return GetQueryable(true, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefault();
            }
            else
            {
                return context.Set<TEntity>().Find(id);
            }
        }

        public virtual TEntity GetByIdNoTracking(object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            Expression<Func<TEntity, bool>> filter = SearchForEntityById(id);
            return GetQueryable(false, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefault();
        }

        public async virtual Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            if (getFullGraph || (includeProperties != null && includeProperties.Count() > 0))
            {
                Expression<Func<TEntity, bool>> filter = SearchForEntityById(id);
                return await GetQueryable(true, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var propType = typeof(TEntity).GetProperty("Id").PropertyType;
                if (id.GetType() != propType)
                {
                    var idConverted = (dynamic)Convert.ChangeType(id, propType);
                    return await context.Set<TEntity>().FindAsync(idConverted).ConfigureAwait(false);
                }
                else
                {
                    return await context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
                }
            }
        }

        public virtual Task<TEntity> GetByIdNoTrackingAsync(CancellationToken cancellationToken, object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            Expression<Func<TEntity, bool>> filter = SearchForEntityById(id);
            return GetQueryable(false, null, null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken);
        }

        public static Expression<Func<TEntity, bool>> SearchForEntityById(object id)
        {
            var item = Expression.Parameter(typeof(TEntity), "entity");
            var prop = Expression.PropertyOrField(item, "Id");
            var propType = typeof(TEntity).GetProperty("Id").PropertyType;

            var value = Expression.Constant((dynamic)Convert.ChangeType(id, propType));

            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, item);
            return lambda;
        }
        #endregion

        #region GetByIdWithPagedCollectionProperty
        public virtual (TEntity Entity, int TotalCount) GetByIdWithPagedCollectionProperty(object id,
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? skip = null,
            int? take = null)
        {
            var entity = GetById(id);
            var count = 0;
            if (entity != null)
            {
                count = context.LoadCollectionProperty(entity, collectionExpression, search, filter, orderBy, skip, take);
            }
            return (entity, count);
        }

        public async virtual Task<(TEntity Entity, int TotalCount)> GetByIdWithPagedCollectionPropertyAsync(CancellationToken cancellationToken, object id,
            string collectionExpression,
            string search = "",
             LambdaExpression filter = null,
            string orderBy = null,
            int? skip = null,
            int? take = null)
        {
            var entity = await GetByIdAsync(cancellationToken, id);
            var count = 0;
            if (entity != null)
            {
                count = await context.LoadCollectionPropertyAsync(entity, collectionExpression, search, filter, orderBy, skip, take, cancellationToken);
            }
            return (entity, count);
        }
        #endregion

        #region GetByIds
        public virtual IReadOnlyList<TEntity> GetByIds(IEnumerable<object> ids,
      bool getFullGraph = false,
      params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = SearchForEntityByIds(list);
            return GetQueryable(true, null, null, filter, null, null).ToList();
        }

        public virtual IReadOnlyList<TEntity> GetByIdsNoTracking(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = SearchForEntityByIds(list);
            return GetQueryable(false, null, null, filter, null, null).ToList();
        }

        public async virtual Task<IReadOnlyList<TEntity>> GetByIdsAsync(CancellationToken cancellationToken, IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = SearchForEntityByIds(list);
            return await GetQueryable(false, null, null, filter, null, null).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async virtual Task<IReadOnlyList<TEntity>> GetByIdsNoTrackingAsync(CancellationToken cancellationToken, IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties)
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = SearchForEntityByIds(list);
            return await GetQueryable(false, null, null, filter, null, null).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public static Expression<Func<TEntity, bool>> SearchForEntityByIds(IEnumerable<object> ids)
        {
            var item = Expression.Parameter(typeof(TEntity), "entity");
            var prop = Expression.PropertyOrField(item, "Id");

            var propType = typeof(TEntity).GetProperty("Id").PropertyType;

            var genericType = typeof(List<>).MakeGenericType(propType);
            var idList = Activator.CreateInstance(genericType);

            var add_method = idList.GetType().GetMethod("Add");
            foreach (var id in ids)
            {
                add_method.Invoke(idList, new object[] { (dynamic)Convert.ChangeType(id, propType) });
            }

            var contains_method = idList.GetType().GetMethod("Contains");
            var value_expression = Expression.Constant(idList);
            var contains_expression = Expression.Call(value_expression, contains_method, prop);
            var lamda = Expression.Lambda<Func<TEntity, bool>>(contains_expression, item);
            return lamda;
        }
        #endregion

        #region Exists
        public virtual bool Exists(Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(true, null, null, filter).ToList().Any();
        }

        public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> filter = null)
        {
            return (await GetQueryable(true, null, null, filter).ToListAsync(cancellationToken).ConfigureAwait(false)).Any();
        }

        public virtual bool ExistsNoTracking(Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(false, null, null, filter).Any();
        }

        public virtual Task<bool> ExistsNoTrackingAsync(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(false, null, null, filter).AnyAsync(cancellationToken);
        }

        public bool Exists(object id)
        {
            return GetById(id) != null;
        }

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken, object id)
        {
            return (await GetByIdAsync(cancellationToken, id)) != null;
        }

        public bool ExistsNoTracking(object id)
        {
            return GetByIdNoTracking(id) != null;
        }

        public async Task<bool> ExistsNoTrackingAsync(CancellationToken cancellationToken, object id)
        {
            return (await GetByIdNoTrackingAsync(cancellationToken, id)) != null;
        }

        public virtual bool Exists(TEntity entity)
        {
            return context.EntityExists(entity);
        }

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken, TEntity entity)
        {
            return context.EntityExistsAsync(entity, cancellationToken);
        }

        public virtual bool ExistsNoTracking(TEntity entity)
        {
            return context.EntityExistsNoTracking(entity);
        }

        public virtual Task<bool> ExistsNoTrackingAsync(CancellationToken cancellationToken, TEntity entity)
        {
            return context.EntityExistsNoTrackingAsync(entity, cancellationToken);
        }

        public virtual bool ExistsById(object id)
        {
            return context.EntityExistsById<TEntity>(id);
        }

        public virtual Task<bool> ExistsByIdAsync(CancellationToken cancellationToken, object id)
        {
            return context.EntityExistsByIdAsync<TEntity>(id, cancellationToken);
        }

        public virtual bool ExistsByIdNoTracking(object id)
        {
            return context.EntityExistsByIdNoTracking<TEntity>(id);
        }

        public virtual Task<bool> ExistsByIdNoTrackingAsync(CancellationToken cancellationToken, object id)
        {
            return context.EntityExistsByIdNoTrackingAsync<TEntity>(id, cancellationToken);
        }
        #endregion

        #region OrderBy
        public static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy(string orderColumn, string orderType)
        {
            if (string.IsNullOrEmpty(orderColumn))
                return null;

            Type typeQueryable = typeof(IQueryable<TEntity>);
            ParameterExpression argQueryable = Expression.Parameter(typeQueryable, "p");
            var outerExpression = Expression.Lambda(argQueryable, argQueryable);
            string[] props = orderColumn.Split('.');
            IQueryable<TEntity> query = new List<TEntity>().AsQueryable<TEntity>();
            Type type = typeof(TEntity);
            ParameterExpression arg = Expression.Parameter(type, "x");

            Expression expr = arg;
            int i = 0;
            foreach (string prop in props)
            {
                var targetProperty = prop;

                PropertyInfo pi = type.GetProperty(targetProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
                i++;
            }
            LambdaExpression lambda = Expression.Lambda(expr, arg);

            string methodName = (orderType == "asc" || orderType == "OrderBy") ? "OrderBy" : "OrderByDescending";

            var genericTypes = new Type[] { typeof(TEntity), type };

            MethodCallExpression resultExp =
                Expression.Call(typeof(Queryable), methodName, genericTypes, outerExpression.Body, Expression.Quote(lambda));

            var finalLambda = Expression.Lambda(resultExp, argQueryable);

            return ((Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>>)finalLambda).Compile();
        }
        #endregion
    }
}
