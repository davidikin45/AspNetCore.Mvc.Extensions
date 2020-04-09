using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class Utilities
    {
        public static int Count<TSource>(IQueryable<TSource> source)
        {
            return Queryable.Count(source);
        }

        public static IEnumerable ToList(this IQueryable query, Type type)
        {
            var genericListType = typeof(List<>).MakeGenericType(type);
            var genericTaskType = typeof(Task<>).MakeGenericType(genericListType);

            var resultTask = (typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.ToListAsync)).MakeGenericMethod(type).Invoke(null, new object[] { query, CancellationToken.None }));
            var result = genericTaskType.GetProperty("Result").GetValue(resultTask);

            return (IEnumerable)result;
        }

        public static IOrderedQueryable<T> QueryableOrderBy<T>(this IQueryable<T> items, string property, bool ascending)
        {
            var MyObject = Expression.Parameter(typeof(T), "MyObject");
            var MyEnumeratedObject = Expression.Parameter(typeof(IQueryable<T>), "MyQueryableObject");
            var MyProperty = Expression.Property(MyObject, property);
            var MyLamda = Expression.Lambda(MyProperty, MyObject);
            var MyMethod = Expression.Call(typeof(Queryable), ascending ? "OrderBy" : "OrderByDescending", new[] { typeof(T), MyLamda.Body.Type }, MyEnumeratedObject, MyLamda);
            var MySortedLamda = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(MyMethod, MyEnumeratedObject).Compile();
            return MySortedLamda(items);
        }

        public static IQueryable<T> CreateSearchQuery<T>(IQueryable<T> query, string values)
        {
            List<Expression> andExpressions = new List<Expression>();

            ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

            MethodInfo contains_method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            var ignore = new List<string>() { "UserDeleted" };

            foreach (var value in values.Split('&'))
            {
                List<Expression> orExpressions = new List<Expression>();

                foreach (PropertyInfo prop in typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string) && !ignore.Contains(x.Name)))
                {
                    MemberExpression member_expression = Expression.PropertyOrField(parameter, prop.Name);

                    ConstantExpression value_expression = Expression.Constant(value, typeof(string));

                    MethodCallExpression contains_expression = Expression.Call(member_expression, contains_method, value_expression);

                    orExpressions.Add(contains_expression);
                }

                if (orExpressions.Count == 0)
                    return query;

                Expression or_expression = orExpressions[0];

                for (int i = 1; i < orExpressions.Count; i++)
                {
                    or_expression = Expression.OrElse(or_expression, orExpressions[i]);
                }

                andExpressions.Add(or_expression);
            }

            if (andExpressions.Count == 0)
                return query;

            Expression and_expression = andExpressions[0];

            for (int i = 1; i < andExpressions.Count; i++)
            {
                and_expression = Expression.AndAlso(and_expression, andExpressions[i]);
            }

            Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(
                and_expression, parameter);

            return query.Where(expression);
        }

        public static Expression<Func<TEntity, bool>> SearchForEntityByIds<TEntity>(IEnumerable<object> ids)
        {
            var item = Expression.Parameter(typeof(TEntity), "entity");
            var prop = Expression.PropertyOrField(item, "Id");

            var propType = typeof(TEntity).GetProperty("Id").PropertyType;

            var genericType = typeof(List<>).MakeGenericType(propType);
            var idList = Activator.CreateInstance(genericType);

            var add_method = idList.GetType().GetMethod("Add");
            foreach (var id in ids)
            {
                add_method.Invoke(idList, new object[] { (dynamic)Convert.ChangeType(id, propType) });
            }

            var contains_method = idList.GetType().GetMethod("Contains");
            var value_expression = Expression.Constant(idList);
            var contains_expression = Expression.Call(value_expression, contains_method, prop);
            var lamda = Expression.Lambda<Func<TEntity, bool>>(contains_expression, item);
            return lamda;
        }

        public static Object SearchForEntityByIds(Type type, IEnumerable<object> ids)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(new[] { type, typeof(bool) });

            var item = Expression.Parameter(type, "entity");
            var prop = Expression.PropertyOrField(item, "Id");

            var propType = type.GetProperty("Id").PropertyType;

            var genericType = typeof(List<>).MakeGenericType(propType);
            var idList = Activator.CreateInstance(genericType);

            var add_method = idList.GetType().GetMethod("Add");
            foreach (var id in ids)
            {
                add_method.Invoke(idList, new object[] { (dynamic)Convert.ChangeType(id, propType) });
            }

            var contains_method = idList.GetType().GetMethod("Contains");
            var value_expression = Expression.Constant(idList);
            var contains_expression = Expression.Call(value_expression, contains_method, prop);

            return typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Lambda)).MakeGenericMethod(funcType).Invoke(null, new object[] { contains_expression, new ParameterExpression[] { item } });
        }

        public static Expression<Func<TEntity, bool>> SearchForEntityById<TEntity>(object id)
        {
            var item = Expression.Parameter(typeof(TEntity), "entity");
            var prop = Expression.PropertyOrField(item, "Id");
            var propType = typeof(TEntity).GetProperty("Id").PropertyType;

            var value = Expression.Constant((dynamic)Convert.ChangeType(id, propType));

            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, item);
            return lambda;
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

        public static object SearchForEntityById(Type type, object id)
        {
            Type funcType = typeof(Func<,>).MakeGenericType(new[] { type, typeof(bool) });

            var item = Expression.Parameter(type, "entity");
            var prop = Expression.PropertyOrField(item, "Id");
            var propType = type.GetProperty("Id").PropertyType;

            var value = Expression.Constant((dynamic)Convert.ChangeType(id, propType));

            var equal = Expression.Equal(prop, value);

            return typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Lambda)).MakeGenericMethod(funcType).Invoke(null, new object[] { equal, new ParameterExpression[] { item } });
        }

        public static Task<int> CountEFCoreAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(source, cancellationToken);
        }
    }
}
