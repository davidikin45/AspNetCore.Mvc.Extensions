using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByString<T>(this IQueryable<T> source, string orderBy)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                throw new ArgumentNullException(nameof(orderBy));
            }

            IOrderedQueryable<T> result = null;

            // the orderBy string is separated by ",", so we split it.
            var orderByAfterSplit = orderBy.Split(',');

            // apply each orderby clause in reverse order - otherwise, the 
            // IQueryable will be ordered in the wrong order
            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                // trim the orderBy clause, as it might contain leading
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var trimmedOrderByClause = orderByClause.Trim();

                // if the sort option ends with with " desc", we order
                // descending, ortherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // remove " asc" or " desc" from the orderBy clause, so we 
                // get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                if (result == null)
                {
                    result = source.OrderBy(propertyName +
                          (orderDescending ? " descending" : " ascending"));
                }
                else
                {
                    result = result.OrderBy(propertyName +
                         (orderDescending ? " descending" : " ascending"));
                }
            }

            return result;
        }

        public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, int page, int pageSize) where T : class
        {
            var skip = (page - 1) * pageSize;
            return PagedList<T>.Create(query, skip, pageSize);
        }

        public static Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default) where T : class
        {
            var skip = (page - 1) * pageSize;
            return PagedList<T>.CreateAsync(query, skip, pageSize, cancellationToken);
        }

        public static CountList<T> ToCountList<T>(this IQueryable<T> query, int? skip, int? take) where T : class
        {
            return CountList<T>.Create(query, skip, take);
        }

        public static Task<CountList<T>> ToCountListAsync<T>(this IQueryable<T> query, int? skip, int? take, CancellationToken cancellationToken = default) where T : class
        {
            return CountList<T>.CreateAsync(query, skip, take, cancellationToken);
        }

        public static string ToSqlString(this IQueryable query)
        {
            return query.ToString();
        }
    }
}
