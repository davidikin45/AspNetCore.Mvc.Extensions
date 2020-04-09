using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection)
            {
                action(item);
            }
            return collection;
        }

        public static List<T> MergeLists<T>(
          this IEnumerable<List<T>> source)
        {
            var newList = new List<T>();
            foreach (var list in source)
            {
                foreach (var item in list)
                {
                    newList.Add(item);
                }
            }
            return newList;
        }
    }
}
