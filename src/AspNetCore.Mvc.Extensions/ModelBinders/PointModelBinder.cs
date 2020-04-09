using AspNetCore.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ModelBinders
{
    public class PointModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;

        public PointModelBinder(IModelBinder fallbackBinder)
        {
            if (fallbackBinder == null)
                throw new ArgumentNullException(nameof(fallbackBinder));

            _fallbackBinder = fallbackBinder;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return _fallbackBinder.BindModelAsync(bindingContext);
            }

            var valueAsString = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(valueAsString))
            {
                return _fallbackBinder.BindModelAsync(bindingContext);
            }

            string[] latLongStr = valueAsString.Split(',');

            Point result = GeographyExtensions.CreatePoint(double.Parse(latLongStr[0]), double.Parse(latLongStr[1]));

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }

    public class PointModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        public PointModelBinderProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(Point))
            {
                return new PointModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType, _loggerFactory));
            }
            return null;
        }
    }

    public class PointMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public PointMvcOptionsSetup(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Configure(MvcOptions options)
        {
            var binderToFind = options.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));

            if (binderToFind == null) return;

            var index = options.ModelBinderProviders.IndexOf(binderToFind);
            options.ModelBinderProviders.Insert(index, new PointModelBinderProvider(_loggerFactory));
        }
    }
}
