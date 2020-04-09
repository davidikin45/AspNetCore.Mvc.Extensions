using AspNetCore.Mvc.Extensions.Specification;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.UI
{
    public partial class UserOrderBySpecification
    {
        public static OrderBySpecification<T> Create<T>(string orderBy)
        {
            var spec = new UserOrderBySpecification<T>(orderBy);
            if (spec.IsValid && spec.IsNullExpression)
               return UserOrderBySpecification<T>.Nothing;

            return spec;
        }

        public static OrderBySpecification<T> Create<T>(Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> expression)
        {
            var spec = new UserOrderBySpecification<T>(expression);
            if (spec.IsValid && spec.IsNullExpression)
                return UserOrderBySpecification<T>.Nothing;

            return spec;
        }

        public static OrderBySpecification Create(Type type, string orderBy)
        {
            return (OrderBySpecification)typeof(UserOrderBySpecification).GetMethod("Create", new Type[] { typeof(string) }).MakeGenericMethod(type).Invoke(null, new object[] { orderBy });
        }
    }

    public class UserOrderBySpecification<T> : OrderBySpecification<T>
    {
        private readonly Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> _expression;
        public bool IsNullExpression { get; } = true;
        
        //query string
        protected internal UserOrderBySpecification(string orderBy)
        {
            OrderByString = orderBy;
            IsValid = true;
            try
            {
                if (UIHelper.ValidOrderByFor<T>(orderBy))
                {
                    _expression = UIHelper.GetOrderByIQueryable<T>(orderBy);
                    IsNullExpression = _expression != null;
                    IsValid = true;
                }
                else
                {
                    IsValid = false;
                }
            }
            catch
            {
                IsValid = false;
            }
        }

        protected internal UserOrderBySpecification(Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> expression)
        {
            IsValid = true;
            _expression = expression;
        }

        public override Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> ToExpression()
        {
            if (!IsValid)
                throw new InvalidOperationException();

            return _expression;
        }
    }
}
