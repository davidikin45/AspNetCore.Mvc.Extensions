using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CacheQueryAttribute : CqrsDecoratorAttribute
    {
        public CacheQueryAttribute()
            : base(typeof(CacheQueryDecorator<,>))
        {
        }
    }
}
