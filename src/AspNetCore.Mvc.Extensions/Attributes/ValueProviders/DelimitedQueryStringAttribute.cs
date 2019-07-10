using AspNetCore.Mvc.Extensions.ValueProviders;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.ValueProviders
{
    //[DelimitedQueryString(',', '|')]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DelimitedQueryStringAttribute : Attribute, IResourceFilter
    {
        private readonly char[] delimiters;

        public DelimitedQueryStringAttribute(params char[] delimiters)
        {
            this.delimiters = delimiters;
        }

        /// <summary>
        /// Executes the resource filter. Called after execution of the remainder of the pipeline.
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResourceExecutedContext" />.</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Don't need to do anything.
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.ValueProviderFactories.AddDelimitedValueProviderFactory(this.delimiters);
        }
    }
}
