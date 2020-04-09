using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class LamdaHelper
    {
        public static Expression<Func<TDto, bool>> SearchForDto<TDto>(string property, object propertyValue)
        {
            var item = Expression.Parameter(typeof(TDto), "entity");
            var prop = Expression.PropertyOrField(item, property);
            var propType = typeof(TDto).GetProperty(property).PropertyType;

            var value = Expression.Constant((dynamic)Convert.ChangeType(propertyValue, propType));

            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TDto, bool>>(equal, item);
            return lambda;
        }

        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> items, string property, bool ascending)
        {
            var MyObject = Expression.Parameter(typeof(T), "MyObject");
            var MyEnumeratedObject = Expression.Parameter(typeof(IEnumerable<T>), "MyEnumeratedObject");
            var MyProperty = Expression.Property(MyObject, property);
            var MyLamda = Expression.Lambda(MyProperty, MyObject);
            var MyMethod = Expression.Call(typeof(Enumerable), ascending ? "OrderBy" : "OrderByDescending", new[] { typeof(T), MyLamda.Body.Type }, MyEnumeratedObject, MyLamda);
            var MySortedLamda = Expression.Lambda<Func<IEnumerable<T>, IOrderedEnumerable<T>>>(MyMethod, MyEnumeratedObject).Compile();
            return MySortedLamda(items);
        }

        public static Object SearchForEntityByProperty(Type type, string property, IEnumerable<object> values)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(new[] { type, typeof(bool) });

            var item = Expression.Parameter(type, "entity");
            var prop = Expression.PropertyOrField(item, property);

            var propType = type.GetProperty(property).PropertyType;

            var genericType = typeof(List<>).MakeGenericType(propType);
            var idList = Activator.CreateInstance(genericType);

            var add_method = idList.GetType().GetMethod("Add");
            foreach (var value in values)
            {
                add_method.Invoke(idList, new object[] { (dynamic)Convert.ChangeType(value, propType) });
            }

            var contains_method = idList.GetType().GetMethod("Contains");
            var value_expression = Expression.Constant(idList);
            var contains_expression = Expression.Call(value_expression, contains_method, prop);

            return typeof(LamdaHelper).GetMethod(nameof(Lambda)).MakeGenericMethod(funcType).Invoke(null, new object[] { contains_expression, new ParameterExpression[] { item } });
        }

        public static object SearchForEntityByIdCompile(Type type, object id)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(new[] { type, typeof(bool) });

            var item = Expression.Parameter(type, "entity");
            var prop = Expression.PropertyOrField(item, "Id");
            var propType = type.GetProperty("Id").PropertyType;

            var value = Expression.Constant((dynamic)Convert.ChangeType(id, propType));

            var equal = Expression.Equal(prop, value);

            return typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.LambdaCompile)).MakeGenericMethod(funcType).Invoke(null, new object[] { equal, new ParameterExpression[] { item } });
        }

        public static int Count<TSource>(IQueryable<TSource> source)
        {
            return Queryable.Count(source);
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<TDelegate>(body, parameters);
        }

        public static TDelegate LambdaCompile<TDelegate>(Expression body, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<TDelegate>(body, parameters).Compile();
        }

        public static IQueryable<TSource> Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Queryable.Where(source, predicate);
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Enumerable.FirstOrDefault(source, predicate);
        }


    }
}
