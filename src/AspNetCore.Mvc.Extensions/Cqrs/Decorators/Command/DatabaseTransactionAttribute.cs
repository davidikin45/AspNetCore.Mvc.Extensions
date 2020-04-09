using System;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DatabaseTransactionAttribute : CqrsDecoratorAttribute
    {
        public DatabaseTransactionAttribute()
           : base(typeof(DatabaseTransactionDecorator<,>))
        {
        }
    }
}
