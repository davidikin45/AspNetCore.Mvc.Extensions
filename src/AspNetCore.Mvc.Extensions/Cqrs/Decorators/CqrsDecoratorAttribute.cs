using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class CqrsDecoratorAttribute : Attribute
    {
        public CqrsDecoratorAttribute(Type type)
        {
            ImplementationType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Type ImplementationType { get;  }

        public object[] Arguments { get; set; }
    }
}
