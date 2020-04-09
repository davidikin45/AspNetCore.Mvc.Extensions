using AutoMapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public class OrderByMapper : IOrderByMapper
    {
        private readonly OrderByMapperOptions _options;
        public OrderByMapper(IOptions<OrderByMapperOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        public bool ValidOrderByFor<TSource, TDestination>(string orderBy)
        {
            var propertyMapping = GetOrderByMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = orderBy.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }

        public Dictionary<string, OrderByMappingValueCollection<TDestination>> GetOrderByMapping
           <TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = _options.Mappings.OfType<OrderByMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First().MappingDictionary;
            }
            else if (matchingMapping.Count() == 0)
            {
                var mapping = new OrderByMapping<TSource, TDestination>();
                _options.Mappings.GetOrAdd($"{typeof(TSource).Name}:{typeof(TDestination).Name}", mapping);
                return mapping.MappingDictionary;
            }
            else
            {
                throw new Exception($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)},{typeof(TDestination)}");
            }
        }
    }

    public class OrderByMapperOptions
    {
        public ConcurrentDictionary<string, IOrderByMapping> Mappings { get; set; } = new ConcurrentDictionary<string, IOrderByMapping>();
        public OrderByMapping<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            var mapping = new OrderByMapping<TSource, TDestination>();
            Mappings.GetOrAdd($"{typeof(TSource).Name}:{typeof(TDestination).Name}", mapping);
            return mapping;
        }
    }
}
