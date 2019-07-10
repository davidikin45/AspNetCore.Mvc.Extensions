using AspNetCore.Mvc.Extensions.ValueProviders.DelimitedQueryString;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.ValueProviders
{
    //services.AddMvc(options => options.ValueProviderFactories.AddDelimitedValueProviderFactory(',', '|'));
    public static class ValueProviderFactoriesExtensions
    {
        public static void AddDelimitedValueProviderFactory(
            this IList<IValueProviderFactory> valueProviderFactories,
            params char[] delimiters)
        {
            var queryStringValueProviderFactory = valueProviderFactories
                .OfType<QueryStringValueProviderFactory>()
                .FirstOrDefault();
            if (queryStringValueProviderFactory == null)
            {
                valueProviderFactories.Insert(
                    0,
                    new DelimitedQueryStringValueProviderFactory(delimiters));
            }
            else
            {
                valueProviderFactories.Insert(
                    valueProviderFactories.IndexOf(queryStringValueProviderFactory),
                    new DelimitedQueryStringValueProviderFactory(delimiters));
                valueProviderFactories.Remove(queryStringValueProviderFactory);
            }
        }
    }
}
