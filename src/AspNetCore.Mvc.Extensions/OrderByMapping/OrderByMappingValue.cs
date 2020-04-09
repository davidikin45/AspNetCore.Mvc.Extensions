using System;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public class OrderByMappingValue<TDestination>
    {
        public string Name { get; private set; }
        public bool Revert { get; private set; }

        public OrderByMappingValue(Expression<Func<TDestination, object>> propertySelector,
            bool revert = false)
        {
            Name = GetName(propertySelector);
            Revert = revert;
        }

        internal OrderByMappingValue(string name,
            bool revert = false)
        {
            Name = name;
            Revert = revert;
        }

        private string GetName<T>(Expression<Func<T, object>> propertySelector)
        {
            MemberExpression body = (MemberExpression)propertySelector.Body;
            return body.Member.Name;
        }
    }
}
