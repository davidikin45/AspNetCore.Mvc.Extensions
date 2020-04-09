using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public class OrderByMappingValueCollection<TDestination>
    {
        public List<OrderByMappingValue<TDestination>> DestinationProperties { get; set; } = new List<OrderByMappingValue<TDestination>>();

        public OrderByMappingValueCollection<TDestination> MapTo(Expression<Func<TDestination, object>> propertySelector,
            bool revert = false)
        {
            if(propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }

            DestinationProperties.Add(new OrderByMappingValue<TDestination>(propertySelector, revert));
            return this;
        }

        internal OrderByMappingValueCollection<TDestination> MapTo(string name,
            bool revert = false)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            DestinationProperties.Add(new OrderByMappingValue<TDestination>(name, revert));
            return this;
        }
    }
}



