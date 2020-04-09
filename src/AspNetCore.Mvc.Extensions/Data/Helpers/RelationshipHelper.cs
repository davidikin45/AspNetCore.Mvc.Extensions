using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class RelationshipHelper
    {
        //https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
        //1. Never have Many-To-Many relationships. Create a join Entity so the relationship becomes 1-To-Many and Many-To-1.
        //2. Never have 1-To-1 Composition ENTITY Relationships!!! For Composition relationships use Value types instead if required. In EF6 extend from BaseValueObject. In EF Core decorate value type with [OwnedAttribute]. Not sure if this attribute can be applied on base class. ValueTypes are included by default.
        //3. Use collections only for Composition/Owned relationships(1-To-Many, child cannot exist independent of the parent, ) not Aggregation/Associated relationshiships(child can exist independently of the parent, reference relationship). Never use a Collection for Navigation purposes!!!!
        //4. Use Complex properties for Aggregation relationships only (Many-To-1, child can exist independently of the parent, reference relationship). e.g Navigation purposes
        public static List<string> GetAllCompositionRelationshipPropertyIncludes(Type type, int maxDepth = 10)
        {
            return GetAllCompositionAndAggregationRelationshipPropertyIncludes(true, type, null, 0, maxDepth);
        }

        public static List<string> GetAllCompositionAndAggregationRelationshipPropertyIncludes(bool compositionRelationshipsOnly, Type type, string path = null, int depth = 0, int maxDepth = 5)
        {
            List<string> includesList = new List<string>();
            if (depth > maxDepth)
            {
                return includesList;
            }

            List<Type> excludeTypes = new List<Type>()
            {
                typeof(DateTime),
                typeof(String),
                typeof(byte[])
           };

            IEnumerable<PropertyInfo> properties = type.GetProperties().Where(p => p.CanWrite && !p.PropertyType.IsValueType && !excludeTypes.Contains(p.PropertyType) && ((!compositionRelationshipsOnly && !p.PropertyType.IsCollection()) || (p.PropertyType.IsCollection() && type != p.PropertyType.GetGenericArguments().First()))).ToList();

            foreach (var p in properties)
            {
                var includePath = !string.IsNullOrWhiteSpace(path) ? path + "." + p.Name : p.Name;

                includesList.Add(includePath);

                Type propType = null;
                if (p.PropertyType.IsCollection())
                {
                    propType = type.GetGenericArguments(p.Name).First();
                }
                else
                {
                    propType = p.PropertyType;
                }

                includesList.AddRange(GetAllCompositionAndAggregationRelationshipPropertyIncludes(compositionRelationshipsOnly, propType, includePath, depth + 1, maxDepth));
            }

            return includesList;
        }

        public static List<string> GetCollectionExpressionParts(string collectionExpresion)
        {
            return collectionExpresion.Split('/').Select(p => p.Split('[')[0]).ToList();
        }

        public static string GetCollectionExpressionCurrentCollection(string collectionExpresion, Type type)
        {
            var collectionProperty = GetCollectionExpressionParts(collectionExpresion).First();
            return type.GetProperties().Where(p => p.Name.ToUpper() == collectionProperty.ToUpper()).First().Name;
        }

        public static string GetCollectionExpressionNextCollection(string collectionExpresion)
        {
            return String.Join("/", GetCollectionExpressionParts(collectionExpresion).Skip(2).ToArray());
        }

        public static bool CollectionExpressionHasMoreCollections(string collectionExpresion)
        {
            return GetCollectionExpressionParts(collectionExpresion).Count > 2;
        }

        public static string GetCollectionExpressionCurrentCollectionItem(string collectionExpresion)
        {
            var expressionParts = GetCollectionExpressionParts(collectionExpresion);
            if (expressionParts.Count > 1)
            {
                return expressionParts[1];
            }
            return null;
        }

        public static string GetCollectionExpressionWithoutCurrentCollectionItem(string collectionExpresion)
        {
            var expressionParts = GetCollectionExpressionParts(collectionExpresion);
            return String.Join("/", expressionParts.Take(expressionParts.Count - 1));
        }

        public static bool IsCollectionExpressionCollectionItem(string collectionExpresion)
        {
            return GetCollectionExpressionParts(collectionExpresion).Count() % 2 == 0;
        }

        public static bool IsValidCollectionExpression(string collectionExpresion, Type type)
        {
            var collectionExpressionParts = GetCollectionExpressionParts(collectionExpresion);

            if (collectionExpressionParts.Count == 0)
            {
                return false;
            }

            int i = 0;
            foreach (var collectionProperty in collectionExpressionParts)
            {
                if (i % 2 == 0)
                {
                    if (!type.HasProperty(collectionProperty) || !type.IsCollectionProperty(collectionProperty))
                    {
                        return false;
                    }
                    type = type.GetGenericArguments(collectionProperty).Single();
                }
                i++;
            }

            return true;
        }

        public static bool IsValidCollectionItemCreateExpression(string collectionExpresion, Type type)
        {
            var collectionExpressionParts = GetCollectionExpressionParts(collectionExpresion);

            if (collectionExpressionParts.Count == 0)
            {
                return false;
            }

            foreach (var collectionProperty in collectionExpressionParts)
            {
                if (!type.HasProperty(collectionProperty) || !type.IsCollectionProperty(collectionProperty))
                {
                    return false;
                }
                type = type.GetGenericArguments(collectionProperty).Single();
            }

            return true;
        }

        public static object GetCollectionExpressionData(string collectionExpression, Type type, object data)
        {
            if (data == null)
            {
                return null;
            }

            var collectionExpressionParts = GetCollectionExpressionParts(collectionExpression);
            for (int i = 0; i < collectionExpressionParts.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var collection = collectionExpressionParts[i];
                    type = type.GetGenericArguments(collection).Single();
                    data = data.GetPropValue(collection);
                    data = ((IEnumerable<Object>)(typeof(Enumerable).GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(typeof(Object)).Invoke(null, new object[] { data })));
                }
                else
                {
                    var collectionItemId = collectionExpressionParts[i];
                    var whereClause = LamdaHelper.SearchForEntityByIdCompile(type, collectionItemId);
                    data = typeof(LamdaHelper).GetMethod(nameof(LamdaHelper.FirstOrDefault)).MakeGenericMethod(type).Invoke(null, new object[] { data, whereClause });
                }
                if (data == null)
                {
                    return null;
                }
            }

            return data;
        }

        public static Type GetCollectionExpressionType(string collectionExpression, Type type)
        {
            var collectionExpressionParts = GetCollectionExpressionParts(collectionExpression);
            for (int i = 0; i < collectionExpressionParts.Count; i++)
            {
                if (i % 2 == 0)
                {
                    var collection = collectionExpressionParts[i];
                    type = type.GetGenericArguments(collection).Single();
                }
            }

            return type;
        }

        public static Type GetCollectionExpressionCreateType(string collectionExpression, Type type)
        {
            var collectionExpressionParts = GetCollectionExpressionParts(collectionExpression);
            for (int i = 0; i < collectionExpressionParts.Count; i++)
            {
                var collection = collectionExpressionParts[i];
                type = type.GetGenericArguments(collection).Single();
            }

            return type;
        }
    }
}
