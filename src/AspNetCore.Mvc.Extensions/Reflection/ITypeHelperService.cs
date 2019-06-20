using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Reflection
{
    public interface ITypeHelperService
    {
        bool TypeHasProperties(Type type, string fields);
        bool TypeHasProperties<T>(string fields);
    }
}
