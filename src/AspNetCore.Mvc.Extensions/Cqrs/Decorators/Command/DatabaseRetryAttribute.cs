using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DatabaseRetryAttribute : CqrsDecoratorAttribute
    {
        public DatabaseRetryAttribute()
            : base(typeof(DatabaseRetryDecorator<,>))
        {
        }

    }
}
