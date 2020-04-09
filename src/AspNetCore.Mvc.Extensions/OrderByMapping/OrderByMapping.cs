using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public class OrderByMapping<TSource, TDestination> : IOrderByMapping
    {
        public Dictionary<string, OrderByMappingValueCollection<TDestination>> MappingDictionary { get; private set; } = new Dictionary<string, OrderByMappingValueCollection<TDestination>>(StringComparer.OrdinalIgnoreCase);

        public OrderByMapping()
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            foreach (var prop in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if(destinationType.GetProperty(prop.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null)
                {
                    AddOrderBy(prop.Name).MapTo(prop.Name);
                }
            }
        }

        public OrderByMappingValueCollection<TDestination> AddOrderBy(Expression<Func<TDestination, object>> propertySelector)
        {
            var orderBy = GetName(propertySelector);

            if (MappingDictionary.ContainsKey(orderBy))
            {
                MappingDictionary.Remove(orderBy);
            }

            var orderByMappingValueCollection = new OrderByMappingValueCollection<TDestination>();
            MappingDictionary.Add(orderBy, orderByMappingValueCollection);
   
            return MappingDictionary[orderBy];
        }

        private string GetName<T>(Expression<Func<T, object>> propertySelector)
        {
            MemberExpression body = (MemberExpression)propertySelector.Body;
            return body.Member.Name;
        }

        public OrderByMappingValueCollection<TDestination> AddOrderBy(string orderBy)
        {
            if (MappingDictionary.ContainsKey(orderBy))
            {
                MappingDictionary.Remove(orderBy);
            }
             
            var orderByMappingValueCollection = new OrderByMappingValueCollection<TDestination>();
            MappingDictionary.Add(orderBy, orderByMappingValueCollection);
            
            return MappingDictionary[orderBy];
        }
    }
}
