using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class DbSetExtensions
    {
        public static TEntity SingleCache<TEntity>(this DbSet<TEntity> queryable, Expression<Func<TEntity, bool>> condition)
            where TEntity : class
        {
            return queryable.Local.SingleOrDefault(condition.Compile()) // find in local cache
                   ?? queryable.Single(condition); // if local cache returns null check the db
        }

        public static TEntity SingleOrDefaultCache<TEntity>(this DbSet<TEntity> queryable, Expression<Func<TEntity, bool>> condition)
       where TEntity : class
        {
            return queryable.Local.SingleOrDefault(condition.Compile()) // find in local cache
                   ?? queryable.SingleOrDefault(condition); // if local cache returns null check the db
        }

        public static DbContext GetDbContext<T>(this IQueryable<T> dbSet) where T : class
        {
            var infrastructure = dbSet as IInfrastructure<IServiceProvider>;
            var serviceProvider = infrastructure.Instance;
            var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext))
                                       as ICurrentDbContext;
            return currentDbContext.Context;
        }

        public static string GetTableName<T>(this DbSet<T> dbSet) where T : class
        {
            var dbContext = dbSet.GetDbContext();

            var model = dbContext.Model;
            var entityTypes = model.GetEntityTypes();
            var entityType = entityTypes.First(t => t.ClrType == typeof(T));
            var tableNameAnnotation = entityType.GetAnnotation("Relational:TableName");
            var tableName = tableNameAnnotation.Value.ToString();
            return tableName;
        }
    }
}
