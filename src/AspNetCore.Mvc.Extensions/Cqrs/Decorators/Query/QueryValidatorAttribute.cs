using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class QueryValidatorAttribute : CqrsDecoratorAttribute
    {
        public QueryValidatorAttribute()
            : base(typeof(QueryValidatorDecorator<,>))
        {
        }

    }
}
