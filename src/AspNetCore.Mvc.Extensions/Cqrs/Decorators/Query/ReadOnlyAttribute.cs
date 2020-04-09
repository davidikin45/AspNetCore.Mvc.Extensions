using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ReadOnlyAttribute : CqrsDecoratorAttribute
    {
        public ReadOnlyAttribute()
            : base(typeof(ReadOnlyDecorator<,>))
        {
        }
    }
}
