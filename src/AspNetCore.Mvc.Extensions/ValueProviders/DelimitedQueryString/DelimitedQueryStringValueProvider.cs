using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.Globalization;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.ValueProviders.DelimitedQueryString
{
    public class DelimitedQueryStringValueProvider : QueryStringValueProvider
    {
        private readonly CultureInfo culture;
        private readonly char[] delimiters;
        private readonly IQueryCollection queryCollection;

        public DelimitedQueryStringValueProvider(
            BindingSource bindingSource,
            IQueryCollection values,
            CultureInfo culture,
            char[] delimiters)
            : base(bindingSource, values, culture)
        {
            this.queryCollection = values;
            this.culture = culture;
            this.delimiters = delimiters;
        }

        public char[] Delimiters { get { return this.delimiters; } }

        public override ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var values = this.queryCollection[key];
            if (values.Count == 0)
            {
                return ValueProviderResult.None;
            }
            else if (values.Any(x => this.delimiters.Any(y => x.Contains(y))))
            {
                var stringValues = new StringValues(values
                    .SelectMany(x => x.Split(this.delimiters, StringSplitOptions.RemoveEmptyEntries))
                    .ToArray());
                return new ValueProviderResult(stringValues, this.culture);
            }
            else
            {
                return new ValueProviderResult(values, this.culture);
            }
        }
    }
}
