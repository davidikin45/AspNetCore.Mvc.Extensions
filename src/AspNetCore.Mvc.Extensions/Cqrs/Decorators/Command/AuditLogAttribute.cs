using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AuditLogAttribute : CqrsDecoratorAttribute
    {
        public AuditLogAttribute()
            :base(typeof(AuditLoggingDecorator<,>))
        {
        }
    }
}
