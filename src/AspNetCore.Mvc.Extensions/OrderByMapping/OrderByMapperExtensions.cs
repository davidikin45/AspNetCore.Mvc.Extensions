using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public static class OrderByMapperExtensions
    {
        public static Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>> GetOrderByDelegate<TSource, TDestination>(this IOrderByMapper mapper, string orderBy)
        {
            return GetOrderBy<TSource, TDestination>(mapper, orderBy)?.Compile();
        }

        public static Expression<Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>>> GetOrderBy<TSource,TDestination>(this IOrderByMapper mapper, string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
                return null;

            var mapping = mapper.GetOrderByMapping<TSource, TDestination>();

            Expression<Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>>> orderByDelegate = (query) => query.OrderByString(orderBy, mapping);

            return orderByDelegate;
        }
    }
}
