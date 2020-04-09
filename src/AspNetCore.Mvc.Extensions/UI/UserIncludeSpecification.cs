using AspNetCore.Mvc.Extensions.Specification;
using System;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.UI
{
    public partial class UserIncludeSpecification
    {
        public static IncludeSpecification<T> Create<T>(string includes)
        {
            var spec = new UserIncludeSpecification<T>(includes);
            if (spec.IsValid && spec.IsNullExpression)
                return UserIncludeSpecification<T>.Nothing;

            return spec;
        }

        public static IncludeSpecification<T> Create<T>(params Expression<Func<T, Object>>[] expression)
        {
            var spec = new UserIncludeSpecification<T>(expression);
            if (spec.IsValid && spec.IsNullExpression)
                return UserIncludeSpecification<T>.Nothing;

            return spec;
        }

        public static IncludeSpecification Create(Type type, string includes)
        {
            return (IncludeSpecification)typeof(UserIncludeSpecification).GetMethod("Create", new Type[] { typeof(string) }).MakeGenericMethod(type).Invoke(null, new object[] { includes });
        }
    }

    public class UserIncludeSpecification<T> : IncludeSpecification<T>
    {
        private readonly Expression<Func<T, Object>>[] _expression;
        public bool IsNullExpression { get; } = true;
        //query string
        protected internal UserIncludeSpecification(string includes)
        {
            IsValid = true;
            try
            {
                if (UIHelper.ValidIncludesFor<T>(includes))
                {
                    _expression = UIHelper.GetIncludes<T>(includes);
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
        protected internal UserIncludeSpecification(params Expression<Func<T, Object>>[] expression)
        {
            IsValid = true;
            _expression = expression;
        }

        public override Expression<Func<T, Object>>[] ToExpression()
        {
            if (!IsValid)
                throw new InvalidOperationException();

            return _expression;
        }
    }
}
