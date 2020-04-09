using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Mvc.Extensions.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AspNetCore.Mvc.Extensions.UI
{
    //Most times you're going to want Func or Action if all that needs to happen is to run some code. You need Expression when the code needs to be analyzed, serialized, or optimized before it is run. Expression is for thinking about code, Func/Action is for running it.
    //Simply, lambda is an anonymous function which can be passed around in a concise way. A lambda expression represents an anonymous function. It comprises of a set of parameters, a lambda operator (->) and a function body.
    //Why would you use Expression<Func<T>> rather than Func<T>?
    //When you want to treat lambda expressions as expression trees and look inside them instead of executing them. For example, LINQ to SQL gets the expression and converts it to the equivalent SQL statement and submits it to server (rather than executing the lambda).
    //Conceptually, Expression<Func<T>> is completely different from Func<T>. Func<T> denotes a delegate which is pretty much a pointer to a method and Expression<Func<T>> denotes a tree data structure for a lambda expression. This tree structure describes what a lambda expression does rather than doing the actual thing. It basically holds data about the composition of expressions, variables, method calls, ... (for example it holds information such as this lambda is some constant + some parameter). You can use this description to convert it to an actual method (with Expression.Compile) or do other stuff (like the LINQ to SQL example) with it. The act of treating lambdas as anonymous methods and expression trees is purely a compile time thing.
    
    //Includes=include1,include2
    //Where: Name=David&LastName.ew=N
    //OrderBy=Id desc
    //Fields=Name,LastName
    public static class UIHelper
    {
        #region Includes UI

        public static Func<TEntity, Object>[] GetIncludesDelegate<TEntity>(string includes)
        {
            return GetIncludes<TEntity>(includes).Select(i => i.Compile()).ToArray();
        }

        public static Expression<Func<TEntity, Object>>[] GetIncludes<TEntity>(string includes)
        {
            var list = new List<Expression<Func<TEntity, Object>>>();
            foreach (var item in GetIncludes(typeof(TEntity), includes))
            {
                list.Add((Expression<Func<TEntity, Object>>)item);
            }
            return list.ToArray();
        }

        public static LambdaExpression[] GetIncludes(Type typeEntity, string includes)
        {
            if (string.IsNullOrEmpty(includes))
                return new LambdaExpression[0];

            var list = new List<LambdaExpression>();

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = includes.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var propertyName = field.Trim();

                string[] props = propertyName.Split('.');
                Type type = typeEntity;
                ParameterExpression arg = Expression.Parameter(type, "x");

                Expression expr = arg;
                foreach (string prop in props)
                {
                    var targetProperty = prop;

                    PropertyInfo pi = type.GetProperty(targetProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (pi == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeEntity}");
                    }

                    expr = Expression.Property(expr, pi);
                    type = pi.PropertyType;
                }
                LambdaExpression lambda = Expression.Lambda(expr, arg);
                list.Add(lambda);
            }

            return list.ToArray();
        }
        #endregion

        #region Includes UI Validation
        public static bool ValidIncludesFor<TSource>(string includes)
        {
            return ValidIncludesForInner(typeof(TSource), includes);
        }

        public static bool ValidIncludesFor(Type sourceType, string includes)
        {
            return ValidIncludesForInner(sourceType, includes);
        }

        public static bool ValidIncludesFor<TSource, TDestination>(string includes, IMapper mapper)
        {
            return ValidIncludesForInner(typeof(TSource), includes, mapper, typeof(TDestination));
        }

        public static bool ValidIncludesFor(Type sourceType, string includes, IMapper mapper, Type destinationType)
        {
            return ValidIncludesForInner(sourceType, includes, mapper, destinationType);
        }

        private static bool ValidIncludesForInner(Type sourceType, string includes, IMapper mapper = null, Type destinationType = null)
        {
            if (string.IsNullOrWhiteSpace(includes))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = includes.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                var type = sourceType;

                // trim
                var propertyName = field.Trim();

                string[] props = propertyName.Split('.');
                foreach (string prop in props)
                {

                    PropertyInfo pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (pi == null)
                        return false;

                    type = pi.PropertyType;
                }
            }

            if (mapper != null && destinationType != null)
            {
                var getIncludesMethod = typeof(UIHelper).GetMethod(nameof(UIHelper.GetIncludes), new Type[] { sourceType });
                var includesArray = getIncludesMethod.Invoke(null, new object[] { includes });
                var mapIncludesMethod = typeof(AutoMapperHelper).GetMethod(nameof(AutoMapperHelper.MapIncludes), new Type[] { sourceType, destinationType });

                try
                {
                    mapIncludesMethod.Invoke(null, new object[] { mapper, includesArray });
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Json Object (ExpandoObject) > Where Clause UI Collection
        public static Dictionary<string, StringValues> ToDictionaryForFilter(object obj)
        {
            var keyValueContent = ToKeyValueForFilter(obj);
            var queryString = String.Join("&", keyValueContent.Select(kvp => String.Join("&", kvp.Value.Select(v => $"{kvp.Key}={v}"))));
            return QueryHelpers.ParseQuery(queryString);
        }

        public static IDictionary<string, StringValues> ToKeyValueForFilter(object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            // Added by me: avoid cyclic references
            var serializer = new Newtonsoft.Json.JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            JToken token = metaToken as JToken;
            if (token == null)
            {
                // Modified by me: use serializer defined above
                return ToKeyValueForFilter(JObject.FromObject(metaToken, serializer));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, StringValues>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValueForFilter(child);
                    if (childContent != null)
                    {
                        foreach (var item in childContent)
                        {
                            var key = ToCamelCase(item.Key);
                            if (contentData.ContainsKey(key))
                            {
                                contentData[key] = new StringValues(contentData[key].Select(v => v).Concat(item.Value.Select(v => v)).ToArray());
                            }
                            else
                            {
                                contentData.Add(key, item.Value);
                            }
                        }
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                            jValue?.ToString("o", CultureInfo.InvariantCulture) :
                            jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, StringValues> { { arrayRegex.Replace(token.Path, ""), value } };
        }

        private static Regex arrayRegex = new Regex(@"\[\d*\]", RegexOptions.Compiled);

        private static string ToCamelCase(string the_string)
        {
            if (the_string == null || the_string.Length < 2)
                return the_string;

            string[] words = the_string.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }
        #endregion

        #region Where Clause UI
        //https://stackoverflow.com/questions/49944387/how-to-correctly-use-axios-params-with-arrays

        //Equals: /filter/age=5&age10 /filter/age[0].e=5&age[1].e=5
        //Greater Than: /filter/age.gt=5 /filter/age[0].gt=5 
        //Greater Than Equals: /filter/age.gte=5  /filter/age[0].gte=5
        //Less Than: /filter/age.lt=5 /filter/age[0].lt=5
        //Less Than Equals: /filter/age.lte=5 /filter/age[0].lte=5
        //Not Equals: /filter/age.ne=5 /filter/age[0].ne=5
        //Contains: /filter/name.c=Da /filter/age[0].c=Da
        //Starts With: /filter/name.sw=Da /filter/age[0].sw=Da
        //Ends With: /filter/name.ew=Da /filter/age[0].ew=Da

        //Delegate > Func > Predicate 
        public static Func<TEntity, bool> GetFilterDelegate<TEntity>(IEnumerable<KeyValuePair<string, StringValues>> queryString)
        {
            return GetFilter<TEntity>(queryString).Compile();
        }

        public static Func<TEntity, bool> GetFilterDelegate<TEntity>(string queryString)
        {
            return GetFilter<TEntity>(queryString).Compile();
        }

        private class FilterOperation
        {
            public string PropertyName { get; set; }
            public string Operation { get; set; }
            public StringValues Values { get; set; }
        }

        private static readonly Regex Pattern = new Regex(@"^([\w|.]+)(\[\d*\])?(\.(e|gt|gte|lt|lte|ne|c|sw|ew|E|GT|GTE|LT|LTE|NE|C|SW|EW))?(\[\d*\])?$", RegexOptions.Compiled | RegexOptions.RightToLeft);
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo StartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo EndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        public static Expression<Func<TEntity, bool>> GetFilter<TEntity>(IEnumerable<KeyValuePair<string, StringValues>> queryString)
        {
            Type type = typeof(TEntity);
            return (Expression<Func<TEntity, bool>>)GetFilter(type, queryString);
        }

        public static Expression<Func<TEntity, bool>> GetFilter<TEntity>(string queryString)
        {
            Type type = typeof(TEntity);
            return (Expression<Func<TEntity, bool>>)GetFilter(type, queryString);
        }

        public static LambdaExpression GetFilter(Type type, string queryString)
        {
            return GetFilter(type, QueryHelpers.ParseQuery(queryString));
        }

        public static LambdaExpression GetFilter(Type type, IEnumerable<KeyValuePair<string, StringValues>> queryString)
        {
            if (queryString == null || queryString.Count() == 0)
                return null;

            List<Expression> andExpressions = new List<Expression>();
            ParameterExpression parameter = Expression.Parameter(type, "p");

            var filterOperations = new Dictionary<string, FilterOperation>();
            foreach (var kvp in queryString)
            {
                Match m = Pattern.Match(kvp.Key);

                if (m.Success)
                {
                    string propertyName = m.Groups[1].Value.Trim();
                    string operation = m.Groups[4].Value.Trim().ToLower();
                    if (string.IsNullOrEmpty(operation))
                        operation = "e";

                    var key = $"{propertyName}:{operation}";
                    if(filterOperations.ContainsKey(key))
                    {
                        filterOperations[key].Values = new StringValues(filterOperations[key].Values.Select(v => v).Concat(kvp.Value.Select(v => v)).ToArray());
                    }
                    else
                    {
                        filterOperations.Add(key, new FilterOperation() { PropertyName = propertyName, Operation = operation, Values = kvp.Value });
                    }
                }
            }

            foreach (var kvp in filterOperations)
            {
                List<Expression> orExpressions = new List<Expression>();

                string propertyName = kvp.Value.PropertyName;
                string operation = kvp.Value.Operation;

                string[] props = propertyName.Split('.');

                bool valid = true;

                Expression propertyExpression = parameter;
                foreach (string prop in props)
                {
                    var targetProperty = prop;

                    PropertyInfo pi = type.GetProperty(targetProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (pi == null)
                    {
                        valid = false;
                        continue;
                    }

                    propertyExpression = Expression.Property(propertyExpression, pi);
                    type = pi.PropertyType;
                }

                if (!valid)
                    continue;

                Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                foreach (var values in kvp.Value.Values)
                {
                    var valuesSplit = values?.Split("|") ?? new string[] { null };

                    foreach (var value in valuesSplit)
                    {
                        var value_expression = Expression.Constant(value != null ? Convert.ChangeType(value.Trim(), underlyingType) : null, underlyingType);

                        Expression binaryExpression = null;
                        switch (operation)
                        {
                            case "":
                            case "e":
                                binaryExpression = Expression.Equal(propertyExpression, value_expression);
                                break;
                            case "c":
                                binaryExpression = Expression.Call(propertyExpression, ContainsMethod, value_expression);
                                break;
                            case "ne":
                                binaryExpression = Expression.NotEqual(propertyExpression, value_expression);
                                break;
                            case "gt":
                                binaryExpression = Expression.GreaterThan(propertyExpression, value_expression);
                                break;
                            case "lt":
                                binaryExpression = Expression.LessThan(propertyExpression, value_expression);
                                break;
                            case "gte":
                                binaryExpression = Expression.GreaterThanOrEqual(propertyExpression, value_expression);
                                break;
                            case "lte":
                                binaryExpression = Expression.LessThanOrEqual(propertyExpression, value_expression);
                                break;
                            default:
                                break;
                        }

                        orExpressions.Add(binaryExpression);
                    }
                }

                if (orExpressions.Count == 0)
                    continue;

                Expression or_expression = orExpressions[0];

                for (int i = 1; i < orExpressions.Count; i++)
                {
                    if(operation == "ne")
                    {
                        or_expression = Expression.AndAlso(or_expression, orExpressions[i]);
                    }
                    else
                    {
                        or_expression = Expression.OrElse(or_expression, orExpressions[i]);
                    }
                }

                andExpressions.Add(or_expression);
            }

            if (andExpressions.Count == 0)
                return null;

            Expression and_expression = andExpressions[0];

            for (int i = 1; i < andExpressions.Count; i++)
            {
                and_expression = Expression.AndAlso(and_expression, andExpressions[i]);
            }

            //Expression<Func<TEntity, bool>> expression = Expression.Lambda<Func<TEntity, bool>>(and_expression, parameter);
            LambdaExpression expression = Expression.Lambda(and_expression, parameter);

            return expression;
        }
        #endregion

        #region Where Clause UI Validation

        public static bool ValidFilterFor<T>(string queryString)
        {
            return ValidFilterFor(typeof(T), queryString);
        }

        public static bool ValidFilterFor<T>(IEnumerable<KeyValuePair<string, StringValues>> queryString)
        {
            return ValidFilterFor(typeof(T), queryString);
        }

        public static bool ValidFilterFor(Type type, string queryString)
        {
            return ValidFilterFor(type, QueryHelpers.ParseQuery(queryString));
        }

        public static bool ValidFilterFor(Type type, IEnumerable<KeyValuePair<string, StringValues>> queryString)
        {
            if (queryString == null || queryString.Count() == 0)
                return true;

            ParameterExpression parameter = Expression.Parameter(type, "p");

            foreach (var kvp in queryString)
            {
                Match m = Pattern.Match(kvp.Key);

                List<Expression> orExpressions = new List<Expression>();

                if (m.Success)
                {
                    string propertyName = m.Groups[1].Value.Trim();
                    string operation = m.Groups[4].Value.Trim().ToLower();

                    string[] props = propertyName.Split('.');

                    bool valid = true;

                    Expression propertyExpression = parameter;
                    foreach (string prop in props)
                    {
                        var targetProperty = prop;

                        PropertyInfo pi = type.GetProperty(targetProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                        if (pi == null)
                        {
                            valid = false;
                            continue;
                        }

                        propertyExpression = Expression.Property(propertyExpression, pi);
                        type = pi.PropertyType;
                    }

                    if (!valid)
                        continue;

                    Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                    foreach (var values in kvp.Value)
                    {
                        try
                        {
                            var valuesSplit = values?.Split("|") ?? new string[] { null };

                            foreach (var value in valuesSplit)
                            {
                                var value_expression = Expression.Constant(value != null ? Convert.ChangeType(value.Trim(), underlyingType) : null, underlyingType);
                                Expression binaryExpression = null;
                                switch (operation)
                                {
                                    case "":
                                    case "e":
                                        binaryExpression = Expression.Equal(propertyExpression, value_expression);
                                        break;
                                    case "c":
                                        binaryExpression = Expression.Call(propertyExpression, ContainsMethod, value_expression);
                                        break;
                                    case "sw":
                                        binaryExpression = Expression.Call(propertyExpression, StartsWith, value_expression);
                                        break;
                                    case "ew":
                                        binaryExpression = Expression.Call(propertyExpression, EndsWith, value_expression);
                                        break;
                                    case "ne":
                                        binaryExpression = Expression.NotEqual(propertyExpression, value_expression);
                                        break;
                                    case "gt":
                                        binaryExpression = Expression.GreaterThan(propertyExpression, value_expression);
                                        break;
                                    case "lt":
                                        binaryExpression = Expression.LessThan(propertyExpression, value_expression);
                                        break;
                                    case "gte":
                                        binaryExpression = Expression.GreaterThanOrEqual(propertyExpression, value_expression);
                                        break;
                                    case "lte":
                                        binaryExpression = Expression.LessThanOrEqual(propertyExpression, value_expression);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        #endregion

        #region Order By UI
        public static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> GetOrderByIQueryableDelegate<TEntity>(string orderBy)
        {
            return GetOrderByIQueryable<TEntity>(orderBy).Compile();
        }

        public static Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> GetOrderByIQueryable<TEntity>(string orderBy)
        {
            return (Expression<Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>>)GetOrderByIQueryable(typeof(TEntity), orderBy);
        }

        public static LambdaExpression GetOrderByIQueryable(Type typeEntity, string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
                return null;

            Type typeQueryable = typeof(IQueryable<>).MakeGenericType(typeEntity);
            ParameterExpression argQueryable = Expression.Parameter(typeQueryable, "p");
            var outerExpression = Expression.Lambda(argQueryable, argQueryable);

            MethodCallExpression resultExp = null;

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = orderBy.Split(',');

            foreach (var field in fieldsAfterSplit.Reverse())
            {
                // trim
                var trimmedOrderByClause = field.Trim();

                // if the sort option ends with with " desc", we order
                // descending, ortherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc") || trimmedOrderByClause.EndsWith(" descending");

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                string[] props = propertyName.Split('.');
                Type type = typeEntity;
                ParameterExpression arg = Expression.Parameter(type, "x");

                Expression expr = arg;
                foreach (string prop in props)
                {
                    var targetProperty = prop;

                    PropertyInfo pi = type.GetProperty(targetProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    expr = Expression.Property(expr, pi);
                    type = pi.PropertyType;
                }
                LambdaExpression lambda = Expression.Lambda(expr, arg);

                string methodName = !orderDescending ? "OrderBy" : "OrderByDescending";

                var genericTypes = new Type[] { typeEntity, type };

                if (resultExp == null)
                {
                    resultExp =
                    Expression.Call(typeof(Queryable), methodName, genericTypes, outerExpression.Body, Expression.Quote(lambda));
                }
                else
                {
                    resultExp =
                    Expression.Call(typeof(Queryable), methodName, genericTypes, resultExp, Expression.Quote(lambda));
                }
            }

            var finalLambda = Expression.Lambda(resultExp, argQueryable);

            return finalLambda;
        }

        public static Func<IEnumerable<TEntity>, IOrderedEnumerable<TEntity>> GetOrderByIEnumerableDelegate<TEntity>(string orderBy)
        {
            return GetOrderByIEnumerable<TEntity>(orderBy).Compile();
        }

        public static Expression<Func<IEnumerable<TEntity>, IOrderedEnumerable<TEntity>>> GetOrderByIEnumerable<TEntity>(string orderBy)
        {
            return (Expression<Func<IEnumerable<TEntity>, IOrderedEnumerable<TEntity>>>)GetOrderByIEnumerable(typeof(TEntity), orderBy);
        }

        public static LambdaExpression GetOrderByIEnumerable(Type typeEntity, string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
                return null;

            Type typeQueryable = typeof(IEnumerable<>).MakeGenericType(typeEntity);
            ParameterExpression argQueryable = Expression.Parameter(typeQueryable, "p");
            var outerExpression = Expression.Lambda(argQueryable, argQueryable);

            MethodCallExpression resultExp = null;

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = orderBy.Split(',');

            foreach (var field in fieldsAfterSplit.Reverse())
            {
                // trim
                var trimmedOrderByClause = field.Trim();

                // if the sort option ends with with " desc", we order
                // descending, ortherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc") || trimmedOrderByClause.EndsWith(" descending");

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                string[] props = propertyName.Split('.');
                Type type = typeEntity;
                ParameterExpression arg = Expression.Parameter(type, "x");

                Expression expr = arg;
                foreach (string prop in props)
                {
                    var targetProperty = prop;

                    PropertyInfo pi = type.GetProperty(targetProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (pi == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeEntity}");
                    }

                    expr = Expression.Property(expr, pi);
                    type = pi.PropertyType;
                }
                LambdaExpression lambda = Expression.Lambda(expr, arg);

                string methodName = !orderDescending ? "OrderBy" : "OrderByDescending";

                var genericTypes = new Type[] { typeEntity, type };

                if (resultExp == null)
                {
                    resultExp =
                    Expression.Call(typeof(Queryable), methodName, genericTypes, outerExpression.Body, Expression.Quote(lambda));
                }
                else
                {
                    resultExp =
                    Expression.Call(typeof(Queryable), methodName, genericTypes, resultExp, Expression.Quote(lambda));
                }
            }

            var finalLambda = Expression.Lambda(resultExp, argQueryable);

            return finalLambda;
        }
        #endregion

        #region Order By UI Validation
        public static bool ValidOrderByFor<TSource>(string orderBy)
        {
            return ValidOrderByForInner(typeof(TSource), orderBy);
        }

        public static bool ValidOrderByFor(Type sourceType, string orderBy)
        {
            return ValidOrderByForInner(sourceType, orderBy);
        }

        public static bool ValidOrderByFor<TSource, TDestination>(string orderBy, IMapper mapper)
        {
            return ValidOrderByForInner(typeof(TSource), orderBy, mapper, typeof(TDestination));
        }

        public static bool ValidOrderByFor(Type sourceType, string orderBy, IMapper mapper, Type destinationType)
        {
            return ValidOrderByForInner(sourceType, orderBy, mapper, destinationType);
        }

        private static bool ValidOrderByForInner(Type sourceType, string orderBy, IMapper mapper = null, Type destinationType = null)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = orderBy.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                var type = sourceType;

                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                string[] props = propertyName.Split('.');
                foreach (string prop in props)
                {

                    PropertyInfo pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (pi == null)
                        return false;

                    type = pi.PropertyType;
                }
            }

            if (mapper != null && destinationType != null)
            {
                var getOrderByMethod = typeof(UIHelper).GetMethod(nameof(UIHelper.GetOrderByIQueryable), new Type[] { sourceType });
                var orderByIQueryable = getOrderByMethod.Invoke(null, new object[] { orderBy });
                var mapOrderByMethod = typeof(AutoMapperHelper).GetMethod(nameof(AutoMapperHelper.MapOrderBy), new Type[] { sourceType, destinationType });

                try
                {
                    mapOrderByMethod.Invoke(null, new object[] { mapper, orderByIQueryable });
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Fields UI
        public static ExpandoObject ShapeData<TSource>(this TSource source,
  string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var dataShapedObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the ExpandoObject 
                var propertyInfos = typeof(TSource)
                        .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in propertyInfos)
                {
                    // get the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(source);

                    // add the field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }

                return dataShapedObject;
            }

            // the field are separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim each field, as it might contain leading 
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var propertyName = field.Trim();

                // use reflection to get the property on the source object
                // we need to include public and instance, b/c specifying a binding flag overwrites the
                // already-existing binding flags.
                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                }

                // get the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(source);

                // add the field to the ExpandoObject
                ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
            }

            // return
            return dataShapedObject;
        }

        public static ExpandoObject ShapeData(this Object source, Type type,
         string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var dataShapedObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the ExpandoObject 
                var propertyInfos = type.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in propertyInfos)
                {
                    // get the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(source);

                    // add the field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }

                return dataShapedObject;
            }

            // the field are separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim each field, as it might contain leading 
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var propertyName = field.Trim();

                // use reflection to get the property on the source object
                // we need to include public and instance, b/c specifying a binding flag overwrites the
                // already-existing binding flags.
                var propertyInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} wasn't found on {type}");
                }

                // get the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(source);

                // add the field to the ExpandoObject
                ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
            }

            // return
            return dataShapedObject;
        }
        #endregion

        #region Fields UI Collection
        public static IEnumerable<ExpandoObject> ShapeListData<TSource>(
    this IEnumerable<TSource> source,
    string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // create a list to hold our ExpandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            // create a list with PropertyInfo objects on TSource.  Reflection is
            // expensive, so rather than doing it for each object in the list, we do 
            // it once and reuse the results.  After all, part of the reflection is on the 
            // type of the object (TSource), not on the instance
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the ExpandoObject
                var propertyInfos = typeof(TSource)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // only the public properties that match the fields should be
                // in the ExpandoObject

                // the field are separated by ",", so we split it.
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    // trim each field, as it might contain leading 
                    // or trailing spaces. Can't trim the var in foreach,
                    // so use another var.
                    var trimmedField = field.Trim();

                    // remove everything after the first " " - if the fields 
                    // are coming from an orderBy string, this part must be 
                    // ignored
                    var indexOfFirstSpace = trimmedField.IndexOf(" ");
                    var propertyName = indexOfFirstSpace == -1 ?
                        trimmedField : trimmedField.Remove(indexOfFirstSpace);

                    // use reflection to get the property on the source object
                    // we need to include public and instance, b/c specifying a binding flag overwrites the
                    // already-existing binding flags.
                    var propertyInfo = typeof(TSource)
                        .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                    }

                    // add propertyInfo to list 
                    propertyInfoList.Add(propertyInfo);
                }
            }

            // run through the source objects
            foreach (TSource sourceObject in source)
            {
                // create an ExpandoObject that will hold the 
                // selected properties & values
                var dataShapedObject = new ExpandoObject();

                // Get the value of each property we have to return.  For that,
                // we run through the list
                foreach (var propertyInfo in propertyInfoList)
                {
                    // GetValue returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    // add the field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }

                // add the ExpandoObject to the list
                expandoObjectList.Add(dataShapedObject);
            }

            // return the list

            return expandoObjectList;
        }

        public static IEnumerable<ExpandoObject> ShapeListData(
         this IEnumerable<Object> source,
         Type type,
         string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            // create a list to hold our ExpandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            // create a list with PropertyInfo objects on TSource.  Reflection is
            // expensive, so rather than doing it for each object in the list, we do 
            // it once and reuse the results.  After all, part of the reflection is on the 
            // type of the object (TSource), not on the instance
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the ExpandoObject
                var propertyInfos = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // only the public properties that match the fields should be
                // in the ExpandoObject

                // the field are separated by ",", so we split it.
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    // trim each field, as it might contain leading 
                    // or trailing spaces. Can't trim the var in foreach,
                    // so use another var.
                    var propertyName = field.Trim();

                    // use reflection to get the property on the source object
                    // we need to include public and instance, b/c specifying a binding flag overwrites the
                    // already-existing binding flags.
                    var propertyInfo = type
                        .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {type}");
                    }

                    // add propertyInfo to list 
                    propertyInfoList.Add(propertyInfo);
                }
            }

            // run through the source objects
            foreach (Object sourceObject in source)
            {
                if (sourceObject != null)
                {
                    // create an ExpandoObject that will hold the 
                    // selected properties & values
                    var dataShapedObject = new ExpandoObject();

                    // Get the value of each property we have to return.  For that,
                    // we run through the list
                    foreach (var propertyInfo in propertyInfoList)
                    {
                        // GetValue returns the value of the property on the source object
                        var propertyValue = propertyInfo.GetValue(sourceObject);

                        // add the field to the ExpandoObject
                        ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                    }

                    // add the ExpandoObject to the list
                    expandoObjectList.Add(dataShapedObject);
                }
            }

            // return the list

            return expandoObjectList;
        }
        #endregion

        #region Fields UI Validation
        public static bool ValidFieldsFor<T>(string fields)
        {
            return ValidFieldsFor(typeof(T), fields);
        }

        public static bool ValidFieldsFor(Type type, string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the field are separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // check if the requested fields exist on source
            foreach (var field in fieldsAfterSplit)
            {
                // trim each field, as it might contain leading 
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var propertyName = field.Trim();

                // use reflection to check if the property can be
                // found on T. 
                var propertyInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                // it can't be found, return false
                if (propertyInfo == null)
                {
                    return false;
                }
            }

            // all checks out, return true
            return true;
        }
        #endregion
    }
}
