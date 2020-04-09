using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.OrderByMapping
{
    public interface IOrderByMapper
    {
        Dictionary<string, OrderByMappingValueCollection<TDestination>> GetOrderByMapping<TSource, TDestination>();

        bool ValidOrderByFor<TSource, TDestination>(string orderBy);
    }
}