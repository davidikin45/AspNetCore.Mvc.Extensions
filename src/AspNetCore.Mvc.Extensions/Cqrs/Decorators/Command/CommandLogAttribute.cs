using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CommandLogAttribute : CqrsDecoratorAttribute
    {
        public CommandLogAttribute()
            :base(typeof(CommandLoggingDecorator<,>))
        {
        }
    }
}
