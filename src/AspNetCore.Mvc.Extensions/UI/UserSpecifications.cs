using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.UI
{
    public static class UserSpecifications
    {
        public static (IncludeSpecification<T> Includes, FilterSpecification<T> Filter, OrderBySpecification<T> OrderBy, UserFieldsSpecification<T> Fields) Create<T>(string includes, string queryString, string orderBy, string fields)
        {
            return (
                UserIncludeSpecification.Create<T>(includes),
                UserFilterSpecification.Create<T>(queryString),
                UserOrderBySpecification.Create<T>(orderBy),
                UserFieldsSpecification.Create<T>(fields)
                );
        }

        public static (IncludeSpecification<T> Includes, FilterSpecification<T> Filter, OrderBySpecification<T> OrderBy, UserFieldsSpecification<T> Fields) Create<T>(string includes, IEnumerable<KeyValuePair<string, StringValues>> queryCollection, string orderBy, string fields)
        {
            return (
                UserIncludeSpecification.Create<T>(includes),
                UserFilterSpecification.Create<T>(queryCollection),
                UserOrderBySpecification.Create<T>(orderBy),
                UserFieldsSpecification.Create<T>(fields)
                );
        }

        public static (IncludeSpecification<T> Includes, FilterSpecification<T> Filter, OrderBySpecification<T> OrderBy, UserFieldsSpecification<T> Fields) Create<T>(string includes, object queryParamObject, string orderBy, string fields)
        {
            return (
                UserIncludeSpecification.Create<T>(includes),
                UserFilterSpecification.Create<T>(queryParamObject),
                UserOrderBySpecification.Create<T>(orderBy),
                UserFieldsSpecification.Create<T>(fields)
                );
        }

        public static (IncludeSpecification Includes, FilterSpecification Filter, OrderBySpecification OrderBy, UserFieldsSpecification Fields) Create(Type type, string includes, string queryString, string orderBy, string fields)
        {
            return (
                UserIncludeSpecification.Create(type, includes),
                UserFilterSpecification.Create(type, queryString),
                UserOrderBySpecification.Create(type, orderBy),
                UserFieldsSpecification.Create(type, fields)
                );
        }

        public static (IncludeSpecification Includes, FilterSpecification Filter, OrderBySpecification OrderBy, UserFieldsSpecification Fields) Create(Type type, string includes, IEnumerable<KeyValuePair<string, StringValues>> queryCollection, string orderBy, string fields)
        {
            return (
                UserIncludeSpecification.Create(type, includes),
                UserFilterSpecification.Create(type, queryCollection),
                UserOrderBySpecification.Create(type, orderBy),
                UserFieldsSpecification.Create(type, fields)
                );
        }

        public static (IncludeSpecification Includes, FilterSpecification Filter, OrderBySpecification OrderBy, UserFieldsSpecification Fields) Create(Type type, string includes, object queryParamObject, string orderBy, string fields)
        {
            return (
                UserIncludeSpecification.Create(type, includes),
                UserFilterSpecification.Create(type, queryParamObject),
                UserOrderBySpecification.Create(type, orderBy),
                UserFieldsSpecification.Create(type, fields)
                );
        }
    }

}
