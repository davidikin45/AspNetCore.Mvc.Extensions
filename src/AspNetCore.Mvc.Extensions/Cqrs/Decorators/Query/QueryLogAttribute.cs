using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class QueryLogAttribute : CqrsDecoratorAttribute
    {
        public QueryLogAttribute()
            :base(typeof(QueryLoggingDecorator<,>))
        {
        }
    }
}
