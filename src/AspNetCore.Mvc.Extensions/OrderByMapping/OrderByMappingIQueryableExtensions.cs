using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public static class OrderByMappingIQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByString<T>(this IQueryable<T> source, string orderBy, Dictionary<string, OrderByMappingValueCollection<T>> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
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

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                // get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                // Run through the property names in reverse
                // so the orderby clauses are applied in the correct order
                foreach (var destinationProperty in
                    propertyMappingValue.DestinationProperties.AsEnumerable().Reverse())
                {
                    // revert sort order if necessary
                    if (destinationProperty.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    if(result == null)
                    {
                      result = source.OrderBy(destinationProperty.Name +
                        (orderDescending ? " descending" : " ascending"));
                    }
                    else
                    {
                        result = result.OrderBy(destinationProperty.Name +
                            (orderDescending ? " descending" : " ascending"));
                    }
                }
            }

            return result;
        }
    }
}
