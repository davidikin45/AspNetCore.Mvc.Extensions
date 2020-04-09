using AspNetCore.Mvc.Extensions.Data.Helpers;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.Mapping
{
    //Most times you're going to want Func or Action if all that needs to happen is to run some code. You need Expression when the code needs to be analyzed, serialized, or optimized before it is run. Expression is for thinking about code, Func/Action is for running it.
    //Simply, lambda is an anonymous function which can be passed around in a concise way. A lambda expression represents an anonymous function. It comprises of a set of parameters, a lambda operator (->) and a function body.
    //Why would you use Expression<Func<T>> rather than Func<T>?
    //When you want to treat lambda expressions as expression trees and look inside them instead of executing them. For example, LINQ to SQL gets the expression and converts it to the equivalent SQL statement and submits it to server (rather than executing the lambda).
    //Conceptually, Expression<Func<T>> is completely different from Func<T>. Func<T> denotes a delegate which is pretty much a pointer to a method and Expression<Func<T>> denotes a tree data structure for a lambda expression. This tree structure describes what a lambda expression does rather than doing the actual thing. It basically holds data about the composition of expressions, variables, method calls, ... (for example it holds information such as this lambda is some constant + some parameter). You can use this description to convert it to an actual method (with Expression.Compile) or do other stuff (like the LINQ to SQL example) with it. The act of treating lambdas as anonymous methods and expression trees is purely a compile time thing.
    public static class AutoMapperHelper
    {
        public static object SourceDestinationEquivalentExpressionById(Type sourceType, Type destinationType)
        {
            Type funcType = typeof(Func<,,>).MakeGenericType(new[] { sourceType, destinationType, typeof(bool) });

            var itemSource = Expression.Parameter(sourceType, "source");
            var propSource = Expression.PropertyOrField(itemSource, "Id");
            var propTypeSource = sourceType.GetProperty("Id").PropertyType;

            var itemDestination = Expression.Parameter(destinationType, "destination");
            var propDestination = Expression.PropertyOrField(itemDestination, "Id");
            var propTypeDestination = destinationType.GetProperty("Id").PropertyType;

            var equal = Expression.Equal(propSource, propDestination);

            return typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.Lambda)).MakeGenericMethod(funcType).Invoke(null, new object[] { equal, new ParameterExpression[] { itemSource, itemDestination } });
        }

        #region Includes Mapping
        //Expression > Func yes
        //Func > Expression no compiled
        public static Expression<Func<TDestination, Object>>[] MapIncludes<TSource, TDestination>(this IMapper mapper, params Expression<Func<TSource, Object>>[] includes)
        {
            var mappedList = MapIncludes(mapper, typeof(TSource), typeof(TDestination), includes);
            List<Expression<Func<TDestination, Object>>> returnList = new List<Expression<Func<TDestination, Object>>>();

            foreach (var item in mappedList)
            {
                returnList.Add((Expression<Func<TDestination, Object>>)item);
            }

            return returnList.ToArray();
        }

        public static LambdaExpression[] MapIncludes(this IMapper mapper, Type source, Type destination, params LambdaExpression[] includes)
        {
            if (includes == null)
                return new LambdaExpression[] { };

            Expression<Func<bool>> a = () => true;

            List<LambdaExpression> returnList = new List<LambdaExpression>();
            var sourceType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(source, typeof(object)));
            var destinationType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(destination, typeof(object)));

            foreach (var include in includes)
            {
                returnList.Add((LambdaExpression)mapper.Map(include, sourceType, destinationType));
            }

            return returnList.ToArray();
        }
        #endregion

        #region Where Clause Mapping
        public static Expression<Func<TDestination, bool>> MapWhereClause<TSource, TDestination>(this IMapper mapper, Expression<Func<TSource, bool>> whereClause)
        {
            return mapper.Map<Expression<Func<TDestination, bool>>>(whereClause);
        }

        public static LambdaExpression MapWhereClause(this IMapper mapper, Type source, Type destination, LambdaExpression whereClause)
        {
            var sourceType = typeof(Expression).MakeGenericType(typeof(Func<,>).MakeGenericType(source, typeof(bool)));
            var destinationType = typeof(Expression).MakeGenericType(typeof(Func<,>).MakeGenericType(destination, typeof(bool)));
            return (LambdaExpression)mapper.Map(whereClause, sourceType, destinationType);
        }
        #endregion

        #region Order By Mapping
        public static Expression<Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>>> MapOrderBy<TSource, TDestination>(this IMapper mapper, Expression<Func<IQueryable<TSource>, IOrderedQueryable<TSource>>> orderBy)
        {
            if (orderBy == null)
                return null;

            return mapper.Map<Expression<Func<IQueryable<TDestination>, IOrderedQueryable<TDestination>>>>(orderBy);
        }
        #endregion
    }
}
