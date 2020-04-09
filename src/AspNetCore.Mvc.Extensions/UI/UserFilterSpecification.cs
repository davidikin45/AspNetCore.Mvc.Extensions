using AspNetCore.Mvc.Extensions.Specification;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.UI
{
    //Cant use CustomModelingBinding and return ModelBindingResult.Failed() because with API it uses json model binding.

    //https://gunnarpeipman.com/serialize-url-encoded-form-data/
    //https://geeklearning.io/serialize-an-object-to-an-url-encoded-string-in-csharp/
    //https://www.hanselman.com/blog/ASPNETWireFormatForModelBindingToArraysListsCollectionsDictionaries.aspx

    //https://stackoverflow.com/questions/49944387/how-to-correctly-use-axios-params-with-arrays

    //Equals: /filter/age=5 /filter/age=5|6 /filter/age[0].e=5
    //Greater Than: /filter/age.gt=5 /filter/age[0].gt=5 
    //Greater Than Equals: /filter/age.gte=5  /filter/age[0].gte=5
    //Less Than: /filter/age.lt=5 /filter/age[0].lt=5
    //Less Than Equals: /filter/age.lte=5 /filter/age[0].lte=5
    //Not Equals: /filter/age.ne=5 /filter/age.ne=5|6 /filter/age[0].ne=5
    //Contains: /filter/name.c=Da /filter/name.c=Da|Do /filter/age[0].c=Da
    //Starts With: /filter/name.sw=Da /filter/name.sw=Da|Do /filter/age[0].sw=Da
    //Ends With: /filter/name.ew=Da /filter/name.ew=Da|Do /filter/age[0].ew=Da

    //"property": {
    //"e":[5,6],
    //"lt":[100],
    //"lte":[100],
    //"gt":[1],
    //"gte":[1],
    //"ne":[5,2,3],
    //"c":['D','Z'],
    //"sw":['D','Z'],
    //"ew":['D','Z']
    //}

    public partial class UserFilterSpecification
    {
        public static FilterSpecification<T> Create<T>(string queryString)
        {
            var spec = new UserFilterSpecification<T>(queryString);
            if (spec.IsValid && spec.IsNullExpression)
                return FilterSpecification<T>.All;

            return spec;
        }

        public static FilterSpecification<T> Create<T>(IEnumerable<KeyValuePair<string, StringValues>> queryCollection)
        {
            var spec = new UserFilterSpecification<T>(queryCollection);
            if (spec.IsValid && spec.IsNullExpression)
                return FilterSpecification<T>.All;

            return spec;
        }

        public static FilterSpecification<T> Create<T>(object queryParamObject)
        {
            var spec = new UserFilterSpecification<T>(queryParamObject);
            if (spec.IsValid && spec.IsNullExpression)
                return FilterSpecification<T>.All;

            return spec;
        }

        public static FilterSpecification<T> Create<T>(Expression<Func<T, bool>> expression)
        {
            var spec = new UserFilterSpecification<T>(expression);
            if (spec.IsValid && spec.IsNullExpression)
                return FilterSpecification<T>.All;

            return spec;
        }

        public static FilterSpecification Create(Type type, string queryString)
        {
            return (FilterSpecification)typeof(UserFilterSpecification).GetMethod("Create", new Type[] { typeof(string) }).MakeGenericMethod(type).Invoke(null, new object[] { queryString });
        }

        public static FilterSpecification Create(Type type, IEnumerable<KeyValuePair<string, StringValues>> queryCollection)
        {
            return (FilterSpecification)typeof(UserFilterSpecification).GetMethod("Create", new Type[] { typeof(IEnumerable<KeyValuePair<string, StringValues>>) }).MakeGenericMethod(type).Invoke(null, new object[] { queryCollection });
        }

        public static FilterSpecification Create(Type type, object queryParamObject)
        {
            return (FilterSpecification)typeof(UserFilterSpecification).GetMethod("Create", new Type[] { typeof(object) }).MakeGenericMethod(type).Invoke(null, new object[] { queryParamObject });
        }
    }

    public class UserFilterSpecification<T> : FilterSpecification<T>
    {
        private readonly Expression<Func<T, bool>> _expression;
        public bool IsNullExpression { get; } = true;

        protected internal UserFilterSpecification(string queryString)
              : this(QueryHelpers.ParseQuery(queryString))
        {

        }

        //query string
        protected internal UserFilterSpecification(IEnumerable<KeyValuePair<string, StringValues>> queryCollection)
        {
            IsValid = true;
            try
            {
                if (UIHelper.ValidFilterFor<T>(queryCollection))
                {
                    _expression = UIHelper.GetFilter<T>(queryCollection);
                    IsNullExpression = _expression != null;
                    IsValid = true;
                }
                else
                {
                    IsValid = false;
                }
            }
            catch
            {
                IsValid = false;
            }
        }

        //dynamic json. e.g ExpandoObject
        protected internal UserFilterSpecification(object queryParamObject)
            : this(UIHelper.ToDictionaryForFilter(queryParamObject))
        {

        }

        protected internal UserFilterSpecification(Expression<Func<T, bool>> expression)
        {
            IsValid = true;
            _expression = expression;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            if (!IsValid)
                throw new InvalidOperationException();

            return _expression;
        }
    }
}
