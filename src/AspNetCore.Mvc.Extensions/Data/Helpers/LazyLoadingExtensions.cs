using System;
using System.Runtime.CompilerServices;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class LazyLoadingExtensions
    {
        public static TRelated Load<TRelated>(
            this Action<object, string> loader,
            object entity,
            ref TRelated navigationField,
            [CallerMemberName] string navigationName = null)
            where TRelated : class
        {
            loader?.Invoke(entity, navigationName);

            return navigationField;
        }
    }
}
