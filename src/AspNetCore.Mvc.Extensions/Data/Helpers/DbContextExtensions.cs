using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class DbContextExtensions
    {
        #region EntityEntry
        public static int GetKeyValue(this EntityEntry entityEntry)
        {
            var etype = entityEntry.Context.Model.FindEntityType(entityEntry.Entity.GetType());
            var pkey = etype.FindPrimaryKey();
            var props = pkey.Properties;
            var keyName = entityEntry.Context.Model.FindEntityType(entityEntry.Entity.GetType()).FindPrimaryKey().Properties
            .Select(x => x.Name).Single();

            return (int)entityEntry.Entity.GetType().GetProperty(keyName).GetValue(entityEntry.Entity, null);
        }
        #endregion

        #region Interface Methods and Properties

        public static int CachedEntityCount(this DbContext context)
        {
            return context.ChangeTracker.Entries().Count();
        }

        public static bool IsEntityStateAdded(this DbContext context, object entity)
        {
            return context.Entry(entity).State == EntityState.Added;
        }

        public static bool IsEntityStateDeleted(this DbContext context, object entity)
        {
            return context.Entry(entity).State == EntityState.Deleted;
        }

        public static bool IsEntityStateDetached(this DbContext context, object entity)
        {
            return context.Entry(entity).State == EntityState.Detached;
        }

        public static bool IsEntityStateModified(this DbContext context, object entity)
        {
            return context.Entry(entity).State == EntityState.Modified;
        }

        public static bool IsEntityStateUnchanged(this DbContext context, object entity)
        {
            return context.Entry(entity).State == EntityState.Unchanged;
        }

        public static void SetEntityStateAdded(this DbContext context, object entity)
        {
            context.Entry(entity).State = EntityState.Added;
        }

        public static void SetEntityStateDeleted(this DbContext context, object entity)
        {
            context.Entry(entity).State = EntityState.Deleted;
        }

        public static void SetEntityStateDetached(this DbContext context, object entity)
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        public static void SetEntityStateModified(this DbContext context, object entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }

        public static void SetEntityStateUnchanged(this DbContext context, object entity)
        {
            context.Entry(entity).State = EntityState.Unchanged;
        }

        public static IEnumerable<TResultType> SQLQueryNoTracking<TResultType>(this DbContext context, string query, params object[] paramaters) where TResultType : class
        {
            return context.Set<TResultType>().FromSqlRaw(query, paramaters).AsNoTracking().ToList();
            //.NET Core 2.2 
            //return context.Set<TResultType>().AsNoTracking().FromSql(query, paramaters).ToList();
        }

        public static async Task<IEnumerable<TResultType>> SQLQueryNoTrackingAsync<TResultType>(this DbContext context, string query, params object[] paramaters) where TResultType : class
        {
            return await context.Set<TResultType>().FromSqlRaw(query, paramaters).AsNoTracking().ToListAsync();
            //.NET Core 2.2 
            //return await context.Set<TResultType>().AsNoTracking().FromSql(query, paramaters).ToListAsync();
        }

        public static IEnumerable<TResultType> SQLQueryTracking<TResultType>(this DbContext context, string query, params object[] paramaters) where TResultType : class
        {
            return context.Set<TResultType>().FromSqlRaw(query, paramaters).ToList();
            //.NET Core 2.2 
            //return context.Set<TResultType>().FromSql(query, paramaters).ToList();
        }

        public static async Task<IEnumerable<TResultType>> SQLQueryTrackingAsync<TResultType>(this DbContext context, string query, params object[] paramaters) where TResultType : class
        {
            return await context.Set<TResultType>().FromSqlRaw(query, paramaters).ToListAsync();
            //.NET Core 2.2 
            //return await context.Set<TResultType>().FromSql(query, paramaters).ToListAsync();
        }

        //Using this should give performance improvement.
        //https://msdn.microsoft.com/en-us/library/jj592677(v=vs.113).aspx
        //Note that only properties that are set to different values when copied from the other object will be marked as modified.
        public static void UpdateEntity(this DbContext context, object existingEntity, object newEntity)
        {
            context.Entry(existingEntity).CurrentValues.SetValues(newEntity);
        }

        public static void TriggerTrackChanges(this DbContext context, object newEntity)
        {
            var currentValues = context.Entry(newEntity).CurrentValues.Clone();
            context.Entry(newEntity).CurrentValues.SetValues(context.Entry(newEntity).OriginalValues);
            context.Entry(newEntity).CurrentValues.SetValues(currentValues);
        }

        //Only need to call this in disconnected mode.
        public static TEntity UpdateEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            return context.Set<TEntity>().Update(entity).Entity;
        }

        public static TEntity AddOrUpdateEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            return context.Set<TEntity>().Update(entity).Entity;
        }

        public static TEntity AddEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            return context.Set<TEntity>().Add(entity).Entity;
        }

        public static void AttachEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            context.Set<TEntity>().Attach(entity);
        }

        public static void RemoveEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            context.Set<TEntity>().Remove(entity);
        }

        public static IQueryable<TEntity> Queryable<TEntity>(this DbContext context) where TEntity : class
        {
            return context.Set<TEntity>();
        }

        public static IQueryable Queryable(this DbContext context, string entityName) =>
            context.Queryable(context.Model.FindEntityType(entityName).ClrType);

        static readonly MethodInfo SetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set));

        public static IQueryable Queryable(this DbContext context, Type entityType) =>
            (IQueryable)SetMethod.MakeGenericMethod(entityType).Invoke(context, null);
#endregion

#region Collection Property

        //collection/id/collection2
        //collection/id/collection2/id2
        public static int LoadCollectionProperty(this DbContext context, object entity, string collectionExpression, string search = "", LambdaExpression filter = null, string orderBy = null, int? skip = null, int? take = null)
        {
            string collectionProperty = RelationshipHelper.GetCollectionExpressionCurrentCollection(collectionExpression, entity.GetType());
            object collectionItemId = RelationshipHelper.GetCollectionExpressionCurrentCollectionItem(collectionExpression);

            var collectionItemType = entity.GetType().GetGenericArguments(collectionProperty).Single();

            Type iQueryableType = typeof(IQueryable<>).MakeGenericType(new[] { collectionItemType });

            var query = context.Entry(entity)
            .Collection(collectionProperty)
            .Query();

            if (collectionItemId != null)
            {
                var whereClause = Utilities.SearchForEntityById(collectionItemType, collectionItemId);
                query = (IQueryable)typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Where)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, whereClause });
                typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.Load)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query });
            }
            else
            {
                if (!string.IsNullOrEmpty(search))
                {
                    query = (IQueryable)typeof(Utilities).GetMethod(nameof(Utilities.CreateSearchQuery)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, search });
                }

                if (filter != null)
                {
                    query = (IQueryable)typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Where)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, filter });
                }

                var count = ((int)(typeof(Utilities).GetMethod(nameof(Utilities.Count)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query })));

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    query = (IQueryable)typeof(IQueryableExtensions).GetMethod(nameof(IQueryableExtensions.OrderByString)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, orderBy });
                }

                if (skip.HasValue)
                {
                    typeof(Queryable).GetMethod(nameof(System.Linq.Queryable.Skip)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, skip.Value });
                }

                if (take.HasValue)
                {
                    typeof(Queryable).GetMethod(nameof(System.Linq.Queryable.Take)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, take.Value });
                }

                typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.Load)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query });

                return count;
            }

            if (collectionItemId != null && RelationshipHelper.CollectionExpressionHasMoreCollections(collectionExpression))
            {
                //Should only be one
                var items = entity.GetPropValue(collectionProperty) as IEnumerable;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        return context.LoadCollectionProperty(item, RelationshipHelper.GetCollectionExpressionNextCollection(collectionExpression), search, filter, orderBy, skip, take);
                    }
                }
            }

            return 1;
        }

        public static async Task<int> LoadCollectionPropertyAsync(this DbContext context, object entity, string collectionExpression, string search = "", LambdaExpression filter = null, string orderBy = null, int? skip = null, int? take = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string collectionProperty = RelationshipHelper.GetCollectionExpressionCurrentCollection(collectionExpression, entity.GetType());
            object collectionItemId = RelationshipHelper.GetCollectionExpressionCurrentCollectionItem(collectionExpression);

            var collectionItemType = entity.GetType().GetGenericArguments(collectionProperty).Single();

            Type iQueryableType = typeof(IQueryable<>).MakeGenericType(new[] { collectionItemType });

            var query = context.Entry(entity)
            .Collection(collectionProperty)
            .Query();

            if (collectionItemId != null)
            {
                var whereClause = Utilities.SearchForEntityById(collectionItemType, collectionItemId);
                query = (IQueryable)typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Where)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, whereClause });
                await ((Task)(typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.LoadAsync)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, cancellationToken }))).ConfigureAwait(false);
            }
            else
            {
                if (!string.IsNullOrEmpty(search))
                {
                    query = (IQueryable)typeof(Utilities).GetMethod(nameof(Utilities.CreateSearchQuery)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, search });
                }

                if(filter != null)
                {
                    query = (IQueryable)typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Where)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, filter });
                }

                var count =  await (Task<int>)(typeof(Utilities).GetMethod(nameof(Utilities.CountEFCoreAsync)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, cancellationToken }));

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    query = (IQueryable)typeof(IQueryableExtensions).GetMethod(nameof(IQueryableExtensions.OrderByString)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, orderBy });
                }

                if (skip.HasValue)
                {
                    query = (IQueryable)typeof(Queryable).GetMethod(nameof(System.Linq.Queryable.Skip)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, skip.Value });
                }

                if (take.HasValue)
                {
                    query = (IQueryable)typeof(Queryable).GetMethod(nameof(System.Linq.Queryable.Take)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, take.Value });
                }

                await ((Task)(typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.LoadAsync)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, cancellationToken }))).ConfigureAwait(false);

                return count;
            }     

            await ((Task)(typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.LoadAsync)).MakeGenericMethod(collectionItemType).Invoke(null, new object[] { query, cancellationToken }))).ConfigureAwait(false);

            if (collectionItemId != null && RelationshipHelper.CollectionExpressionHasMoreCollections(collectionExpression))
            {
                //Should only be one
                var items = entity.GetPropValue(collectionProperty) as IEnumerable;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        return await context.LoadCollectionPropertyAsync(item, RelationshipHelper.GetCollectionExpressionNextCollection(collectionExpression), search, filter, orderBy, skip, take);
                    }
                }
            }

            return 1;
        }
#endregion

#region Local Entity Cache
        public static bool EntityExistsLocal<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            return context.Set<TEntity>().Local.Any(x => Equals(x, entity));
        }

        public static bool EntityExistsByIdLocal<TEntity>(this DbContext context, object id) where TEntity : class
        {
            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id).Compile();
                return context.Set<TEntity>().Local.Any(filter);
            }
            else
            {
                return false;
            }
        }

        public static TEntity FindEntityByIdLocal<TEntity>(this DbContext context, object id) where TEntity : class
        {
            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id).Compile();
                return context.Set<TEntity>().Local.Where(filter).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public static TEntity FindEntityLocal<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            return context.Set<TEntity>().Local.FirstOrDefault(x => Equals(x, entity));
        }
#endregion

#region Entity By Object
        public static bool EntityExists<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var local = context.EntityExistsLocal(entity);
            if (local)
                return true;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return context.Set<TEntity>().Where(filter).ToList().Any();
            }

            return false;
        }

        public static async Task<bool> EntityExistsAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.EntityExistsLocal(entity);
            if (local)
                return true;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return (await context.Set<TEntity>().Where(filter).ToListAsync(cancellationToken)).Any();
            }

            return false;
        }

        public static bool EntityExistsNoTracking<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var local = context.EntityExistsLocal(entity);
            if (local)
                return true;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return context.Set<TEntity>().AsNoTracking().Where(filter).Any();
            }

            return false;
        }

        public static async Task<bool> EntityExistsNoTrackingAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.EntityExistsLocal(entity);
            if (local)
                return true;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return await context.Set<TEntity>().AsNoTracking().Where(filter).AnyAsync(cancellationToken);
            }

            return false;
        }

        public static TEntity FindEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var local = context.FindEntityLocal<TEntity>(entity);
            if (local != null)
                return local;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return context.Set<TEntity>().Where(filter).SingleOrDefault();
            }

            return null;
        }

        public static async Task<TEntity> FindEntityAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.FindEntityLocal<TEntity>(entity);
            if (local != null)
                return local;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return await context.Set<TEntity>().Where(filter).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        public static TEntity FindEntityNoTracking<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var local = context.FindEntityLocal<TEntity>(entity);
            if (local != null)
                return local;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return context.Set<TEntity>().AsNoTracking().Where(filter).SingleOrDefault();
            }

            return null;
        }

        public static async Task<TEntity> FindEntityNoTrackingAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.FindEntityLocal<TEntity>(entity);
            if (local != null)
                return local;

            if (entity.HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), entity.GetPropValue("Id")))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(entity.GetPropValue("Id"));
                return await context.Set<TEntity>().AsNoTracking().Where(filter).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
#endregion

#region Entity By Id
        public static bool EntityExistsById<TEntity>(this DbContext context, object id) where TEntity : class
        {
            var local = context.EntityExistsByIdLocal<TEntity>(id);
            if (local)
                return true;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return context.Set<TEntity>().Where(filter).ToList().Any();
            }

            return false;
        }

        public static async Task<bool> EntityExistsByIdAsync<TEntity>(this DbContext context, object id, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.EntityExistsByIdLocal<TEntity>(id);
            if (local)
                return true;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return (await context.Set<TEntity>().Where(filter).ToListAsync(cancellationToken)).Any();
            }

            return false;
        }

        public static bool EntityExistsByIdNoTracking<TEntity>(this DbContext context, object id) where TEntity : class
        {
            var local = context.EntityExistsByIdLocal<TEntity>(id);
            if (local)
                return true;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return context.Set<TEntity>().AsNoTracking().Where(filter).Any();
            }

            return false;
        }

        public static async Task<bool> EntityExistsByIdNoTrackingAsync<TEntity>(this DbContext context, object id, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.EntityExistsByIdLocal<TEntity>(id);
            if (local)
                return true;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return await context.Set<TEntity>().AsNoTracking().Where(filter).AnyAsync(cancellationToken);
            }

            return false;
        }

        public static TEntity FindEntityById<TEntity>(this DbContext context, object id) where TEntity : class
        {
            //Will track
            return context.Set<TEntity>().Find(id);
        }

        public static async Task<TEntity> FindEntityByIdAsync<TEntity>(this DbContext context, object id, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.FindEntityByIdLocal<TEntity>(id);
            if (local != null)
                return local;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return await context.Set<TEntity>().Where(filter).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        public static TEntity FindEntityByIdNoTracking<TEntity>(this DbContext context, object id) where TEntity : class
        {
            var local = context.FindEntityByIdLocal<TEntity>(id);
            if (local != null)
                return local;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return context.Set<TEntity>().AsNoTracking().Where(filter).SingleOrDefault();
            }

            return null;
        }

        public static async Task<TEntity> FindEntityByIdNoTrackingAsync<TEntity>(this DbContext context, object id, CancellationToken cancellationToken) where TEntity : class
        {
            var local = context.FindEntityByIdLocal<TEntity>(id);
            if (local != null)
                return local;

            if (typeof(TEntity).HasProperty("Id") && !Equals(typeof(TEntity).GetProperty("Id").PropertyType.DefaultValue(), id))
            {
                var filter = Utilities.SearchForEntityById<TEntity>(id);
                return await context.Set<TEntity>().AsNoTracking().Where(filter).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
#endregion

#region helpers
        private static bool HasProperty(this Type type, string propName)
        {
            return type.GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        private static object DefaultValue(this Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        private static object GetPropValue(this object obj, string propName)
        {
            if (HasProperty(obj, propName))
            {
                return obj.GetType().GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).GetValue(obj, null);
            }
            return null;
        }

        private static bool HasProperty(this object obj, string propName)
        {
            return obj.GetType().GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        private static bool IsCollection(this Type type)
        {
            return type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType).Any(x => x.GetGenericTypeDefinition() == typeof(ICollection<>) && !x.GetGenericArguments().Contains(typeof(Byte)));
        }

        private static Type[] GetGenericArguments(this Type type, string propName)
        {
            if (HasProperty(type, propName))
            {
                return type.GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).PropertyType.GenericTypeArguments;
            }
            return null;
        }
#endregion
    }
}
