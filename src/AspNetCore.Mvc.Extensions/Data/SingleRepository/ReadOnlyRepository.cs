using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Domain;
using AspNetCore.Specification;
using AspNetCore.Specification.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.SingleRepository
{
    //https://cpratt.co/truly-generic-repository/
    //System.Linq.Dynamic.Core
    public abstract class ReadOnlyRepository : IReadOnlyRepository
    {
        protected readonly DbContext context;

        //AsNoTracking() causes EF to bypass cache for reads and writes - Ideal for Web Applications and short lived contexts
        public ReadOnlyRepository(DbContext context)
        {
            this.context = context;
        }

        //Can only pass Expression<Func> into EF as it can convert those to SQL
        //EF cant handle Func<> as they are compiled!
        protected virtual IQueryable<TEntity> GetQueryable<TEntity>(
            bool? tracking,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
            params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            //includeProperties = includeProperties ?? string.Empty;
            IQueryable<TEntity> query = context.Set<TEntity>();
            if (tracking.HasValue)
            {
                if (!tracking.Value)
                {
                    query = query.AsNoTracking();
                }
                else
                {
                    //By default tracking is QueryTrackingBehavior.TrackAll. If the DbContext is set to QueryTrackingBehavior.NoTracking we don't want to allow a user to override this behaviour.
                    query = query.AsTracking();
                }
            }

            //where clause
            if (filter != null)
                query = query.Where(filter);

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

            //.ToQueryString() or .ToString()
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

        private void DebugSQL<TEntity>(IQueryable<TEntity> query)
        {
            var sql = query.ToString();
        }

        #region SQLQuery
        public virtual IReadOnlyList<TEntity> SQLQuery<TEntity>(string query, params object[] paramaters) where TEntity : class
        {

            return context.Set<TEntity>().FromSqlRaw(query, paramaters).AsNoTracking().ToList();

            //.NET Core 2.2 
            //return context.Set<TEntity>().AsNoTracking().FromSql(query, paramaters).ToList();
        }

        public virtual IReadOnlyList<TEntity> SQLQuery<TEntity>(FormattableString query) where TEntity : class
        { 
            return context.Set<TEntity>().FromSqlInterpolated(query).AsNoTracking().ToList();
        }

        public async virtual Task<IReadOnlyList<TEntity>> SQLQueryAsync<TEntity>(string query, params object[] paramaters) where TEntity : class
        {
            return await context.Set<TEntity>().FromSqlRaw(query, paramaters).AsNoTracking().ToListAsync();
            //.NET Core 2.2 
            //return await context.Set<TEntity>().AsNoTracking().FromSql(query, paramaters).ToListAsync();
        }

        public async virtual Task<IReadOnlyList<TEntity>> SQLQueryAsync<TEntity>(FormattableString query) where TEntity : class
        {
            return await context.Set<TEntity>().FromSqlInterpolated(query).AsNoTracking().ToListAsync();
        }
        #endregion

        #region Query - GetAll
        public virtual IReadOnlyList<TEntity> GetAll<TEntity>(
         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         int? skip = null,
         int? take = null,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, null, orderBy, skip, take, getFullGraph, includeProperties).ToList().AsReadOnly();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync<TEntity>(CancellationToken cancellationToken,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return await GetQueryable(null, null, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual IReadOnlyList<TEntity> GetAllNoTracking<TEntity>(
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int? skip = null,
        int? take = null,
        bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, null, orderBy, skip, take, getFullGraph, includeProperties).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return await GetQueryable(false, null, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Query - Get
        public virtual IReadOnlyList<TEntity> Get<TEntity>(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, orderBy, skip, take, getFullGraph, includeProperties).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return await GetQueryable(null, filter, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual IReadOnlyList<TEntity> GetNoTracking<TEntity>(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, filter, orderBy, skip, take, getFullGraph, includeProperties).ToList();
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return await GetQueryable(false, filter, orderBy, skip, take, getFullGraph, includeProperties).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable(false, filter).Count();
        }

        public virtual Task<int> GetCountAsync<TEntity>(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable(false, filter).CountAsync(cancellationToken);
        }
        #endregion

        #region Query - Search
        public virtual CountList<TEntity> Search<TEntity>(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
         bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, orderBy, null, null, getFullGraph, includeProperties).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> SearchAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
           bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, orderBy, null, null, getFullGraph, includeProperties).ToCountListAsync(skip, take);
        }

        public virtual CountList<TEntity> SearchNoTracking<TEntity>(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          //  string includeProperties = null,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, filter, orderBy, null, null, getFullGraph, includeProperties).ToCountList(skip, take);
        }

        public virtual Task<CountList<TEntity>> SearchNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            //string includeProperties = null,
            int? skip = null,
            int? take = null,
            bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, filter, orderBy, null, null, getFullGraph, includeProperties)
                .ToCountListAsync(skip, take, cancellationToken);
        }
        #endregion

        #region Query - IQueryable
        public IQueryable<TEntity> Queryable<TEntity>() where TEntity : class => context.Set<TEntity>();

        static readonly MethodInfo SetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set));
        public IQueryable Queryable(Type entityType) => (IQueryable)SetMethod.MakeGenericMethod(entityType).Invoke(context, null);
        #endregion

        #region Query - Search Specification Pattern 
        public virtual SpecificationDbQuery<TEntity> SpecificationQuery<TEntity>() where TEntity : class
        {
            return new SpecificationDbQuery<TEntity>(context);
        }

        public virtual SpecificationDbQuery<TEntity> SpecificationQuery<TEntity>(
          IncludeSpecification<TEntity> includeSpecification,
          FilterSpecification<TEntity> filterSpecification,
          OrderBySpecification<TEntity> orderBySpecification,
          int? skip = null,
          int? take = null,
          bool getFullGraph = false) where TEntity : class
        {
            var specification = new SpecificationDbQuery<TEntity>(context).Include(includeSpecification).Where(filterSpecification).OrderBy(orderBySpecification);

            if (skip.HasValue)
                specification = specification.Skip(skip.Value);

            if (take.HasValue)
                specification = specification.Take(take.Value);

            if (getFullGraph)
                specification = specification.FullGraph();

            return specification;
        }
        #endregion

        #region Query - GetOne
        public virtual TEntity GetOne<TEntity>(
         Expression<Func<TEntity, bool>> filter = null,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, null, null, null, getFullGraph).SingleOrDefault();
        }

        public virtual Task<TEntity> GetOneAsync<TEntity>(CancellationToken cancellationToken,
          Expression<Func<TEntity, bool>> filter = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken);
        }

        public virtual TEntity GetOneNoTracking<TEntity>(
         Expression<Func<TEntity, bool>> filter = null,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefault();
        }

        public virtual Task<TEntity> GetOneNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
          Expression<Func<TEntity, bool>> filter = null,
          bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken);
        }
        #endregion

        #region Query - GetFirst
        public virtual TEntity GetFirst<TEntity>(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefault();
        }

        public virtual Task<TEntity> GetFirstAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(null, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual TEntity GetFirstNoTracking<TEntity>(
         Expression<Func<TEntity, bool>> filter = null,
         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool getFullGraph = false,
        params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            return GetQueryable(false, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefault();
        }

        public virtual Task<TEntity> GetFirstNoTrackingAsync<TEntity>(CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         bool getFullGraph = false,
          params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        { 
            return GetQueryable(false, filter, orderBy, null, null, getFullGraph, includeProperties).FirstOrDefaultAsync(cancellationToken);
        }
        #endregion

        #region GetById
        public virtual TEntity GetById<TEntity>(object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            if (getFullGraph || (includeProperties != null && includeProperties.Count() > 0))
            {
                Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(id);
                return GetQueryable(null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefault();
            }
            else
            {
                var propType = typeof(TEntity).GetProperty("Id").PropertyType;
                if (id.GetType() != propType)
                {
                    var idConverted = (dynamic)Convert.ChangeType(id, propType);
                    return context.Set<TEntity>().Find(idConverted);
                }
                else
                {
                    return context.Set<TEntity>().Find(id);
                }
            }
        }

        public virtual TEntity GetByIdNoTracking<TEntity>(object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(id);
            return GetQueryable(false, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefault();
        }

        public async virtual Task<TEntity> GetByIdAsync<TEntity>(CancellationToken cancellationToken, object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            if (getFullGraph || (includeProperties != null && includeProperties.Count() > 0))
            {
                Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(id);
                return await GetQueryable(null, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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

        public virtual Task<TEntity> GetByIdNoTrackingAsync<TEntity>(CancellationToken cancellationToken, object id, bool getFullGraph = false, params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(id);
            return GetQueryable(false, filter, null, null, null, getFullGraph, includeProperties).SingleOrDefaultAsync(cancellationToken);
        }
        #endregion

        #region GetByIdWithPagedCollectionProperty
        public virtual (TEntity Entity, int TotalCount) GetByIdWithPagedCollectionProperty<TEntity>(object id,
            string collectionExpression,
            string search = "",
            LambdaExpression filter = null,
            string orderBy = null,
            int? skip = null,
            int? take = null) where TEntity : class
        {
            var entity = GetById<TEntity>(id);
            var count = 0;
            if (entity != null)
            {
                count = context.LoadCollectionProperty(entity, collectionExpression, search, filter, orderBy, skip, take);
            }
            return (entity, count);
        }

        public async virtual Task<(TEntity Entity, int TotalCount)> GetByIdWithPagedCollectionPropertyAsync<TEntity>(CancellationToken cancellationToken, object id,
            string collectionExpression,
            string search = "",
             LambdaExpression filter = null,
            string orderBy = null,
            int? skip = null,
            int? take = null) where TEntity : class
        {
            var entity = await GetByIdAsync<TEntity>(cancellationToken, id);
            var count = 0;
            if (entity != null)
            {
                count = await context.LoadCollectionPropertyAsync(entity, collectionExpression, search, filter, orderBy, skip, take, cancellationToken);
            }
            return (entity, count);
        }
        #endregion

        #region GetByIds
        public virtual IReadOnlyList<TEntity> GetByIds<TEntity>(IEnumerable<object> ids,
      bool getFullGraph = false,
      params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(list);
            return GetQueryable(null, filter, null, null).ToList();
        }

        public virtual IReadOnlyList<TEntity> GetByIdsNoTracking<TEntity>(IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(list);
            return GetQueryable(false, filter, null, null).ToList();
        }

        public async virtual Task<IReadOnlyList<TEntity>> GetByIdsAsync<TEntity>(CancellationToken cancellationToken, IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(list);
            return await GetQueryable(false, filter, null, null).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async virtual Task<IReadOnlyList<TEntity>> GetByIdsNoTrackingAsync<TEntity>(CancellationToken cancellationToken, IEnumerable<object> ids,
         bool getFullGraph = false,
         params Expression<Func<TEntity, Object>>[] includeProperties) where TEntity : class
        {
            var list = new List<object>();
            foreach (object id in ids)
            {
                list.Add(id);
            }

            Expression<Func<TEntity, bool>> filter = PredicateEntityById<TEntity>(list);
            return await GetQueryable(false, filter, null, null).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Search Predicates
        public Expression<Func<TEntity, bool>> PredicateEntityById<TEntity>(object id)
        {
            var item = Expression.Parameter(typeof(TEntity), "entity");
            var prop = Expression.PropertyOrField(item, "Id");
            var propType = typeof(TEntity).GetProperty("Id").PropertyType;

            var value = Expression.Constant((dynamic)Convert.ChangeType(id, propType));

            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, item);
            return lambda;
        }

        public Expression<Func<TEntity, bool>> PredicateEntityById<TEntity>(IEnumerable<object> ids)
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

        private static MethodInfo contains_method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        public Expression<Func<TEntity, bool>> PredicateEntityByStringContains<TEntity>(string values)
        {
            if (string.IsNullOrEmpty(values))
                return null;

            List<Expression> andExpressions = new List<Expression>();

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "p");

            var ignore = new List<string>() { "" };

            var entityType = typeof(TEntity);

            var properties = entityType.GetProperties().Where(p => p.PropertyType == typeof(string) && !ignore.Contains(p.Name));

            //.NET Core 2.2
            //var properties = entityType.GetProperties().Where(p => p.ClrType == typeof(string) && !p.IsShadowProperty && !ignore.Contains(p.Name));

            foreach (var value in values.Split('&'))
            {
                List<Expression> orExpressions = new List<Expression>();

                foreach (PropertyInfo prop in properties)
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
                    return null;

                Expression or_expression = orExpressions[0];

                for (int i = 1; i < orExpressions.Count; i++)
                {
                    or_expression = Expression.OrElse(or_expression, orExpressions[i]);
                }

                andExpressions.Add(or_expression);
            }

            if (andExpressions.Count == 0)
                return null;

            Expression and_expression = andExpressions[0];

            for (int i = 1; i < andExpressions.Count; i++)
            {
                and_expression = Expression.AndAlso(and_expression, andExpressions[i]);
            }

            Expression<Func<TEntity, bool>> expression = Expression.Lambda<Func<TEntity, bool>>(
                and_expression, parameter);

            return expression;
        }

        public Expression<Func<TEntity, bool>> And<TEntity>(Expression<Func<TEntity, bool>> leftExpression, Expression<Func<TEntity, bool>> rightExpression)
        {
            if (leftExpression == null)
                return rightExpression;

            BinaryExpression andExpression = Expression.AndAlso(leftExpression.Body, rightExpression.Body);

            return Expression.Lambda<Func<TEntity, bool>>(andExpression, leftExpression.Parameters.Single());
        }

        public Expression<Func<TEntity, bool>> Or<TEntity>(Expression<Func<TEntity, bool>> leftExpression, Expression<Func<TEntity, bool>> rightExpression)
        {
            if (leftExpression == null)
                return rightExpression;

            BinaryExpression orExpression = Expression.OrElse(leftExpression.Body, rightExpression.Body);

            return Expression.Lambda<Func<TEntity, bool>>(orExpression, leftExpression.Parameters.Single());
        }
        #endregion

        #region Query - Exists
        public virtual bool Exists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable(null, filter).ToList().Any();
        }
         
        public virtual async Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return (await GetQueryable(null, filter).ToListAsync(cancellationToken).ConfigureAwait(false)).Any();
        }

        public virtual bool ExistsNoTracking<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable(false, filter).Any();
        }

        public virtual Task<bool> ExistsNoTrackingAsync<TEntity>(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable(false, filter).AnyAsync(cancellationToken);
        }

        public bool Exists<TEntity>(object id) where TEntity : class
        {
            return GetById<TEntity>(id) != null;
        }

        #endregion

        #region Exists by Id or Object
        public async Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken, object id) where TEntity : class
        {
            return (await GetByIdAsync<TEntity>(cancellationToken, id)) != null;
        }

        public bool ExistsNoTracking<TEntity>(object id) where TEntity : class
        {
            return GetByIdNoTracking<TEntity>(id) != null;
        }

        public async Task<bool> ExistsNoTrackingAsync<TEntity>(CancellationToken cancellationToken, object id) where TEntity : class
        {
            return (await GetByIdNoTrackingAsync<TEntity>(cancellationToken, id)) != null;
        }

        public virtual bool Exists<TEntity>(TEntity entity) where TEntity : class
        {
            return context.EntityExists(entity);
        }

        public virtual Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken, TEntity entity) where TEntity : class
        {
            return context.EntityExistsAsync(entity, cancellationToken);
        }

        public virtual bool ExistsNoTracking<TEntity>(TEntity entity) where TEntity : class
        {
            return context.EntityExistsNoTracking(entity);
        }

        public virtual Task<bool> ExistsNoTrackingAsync<TEntity>(CancellationToken cancellationToken, TEntity entity) where TEntity : class
        {
            return context.EntityExistsNoTrackingAsync(entity, cancellationToken);
        }

        public virtual bool ExistsById<TEntity>(object id) where TEntity : class
        {
            return context.EntityExistsById<TEntity>(id);
        }

        public virtual Task<bool> ExistsByIdAsync<TEntity>(CancellationToken cancellationToken, object id) where TEntity : class
        {
            return context.EntityExistsByIdAsync<TEntity>(id, cancellationToken);
        }

        public virtual bool ExistsByIdNoTracking<TEntity>(object id) where TEntity : class
        {
            return context.EntityExistsByIdNoTracking<TEntity>(id);
        }

        public virtual Task<bool> ExistsByIdNoTrackingAsync<TEntity>(CancellationToken cancellationToken, object id) where TEntity : class
        {
            return context.EntityExistsByIdNoTrackingAsync<TEntity>(id, cancellationToken);
        }
        #endregion

        #region OrderBy
        public static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy<TEntity>(string orderColumn, string orderType) where TEntity : class
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
