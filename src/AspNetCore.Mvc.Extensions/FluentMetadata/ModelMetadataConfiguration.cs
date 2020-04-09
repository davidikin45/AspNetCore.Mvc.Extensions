using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public abstract class ModelMetadataConfiguration<TModel> : IModelMetadataConfiguration where TModel : class
    {
        private readonly IDictionary<ModelMetadataIdentity, IMetadataConfigurator> _configurators;
        protected ModelMetadataConfiguration()
        {
            _configurators = new Dictionary<ModelMetadataIdentity, IMetadataConfigurator>();
        }

        public Type ModelType { get; } = typeof(TModel);

        protected IModelMetadataBuilder<TValue> Configure<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var propertyInfo = GetPropertyInfo(expression);
            return GetOrCreateDisplayMetadataConfigurator<TValue>(propertyInfo);
        }
        private PropertyInfo GetPropertyInfo<TSource, TValue>(
             Expression<Func<TSource, TValue>> expression)
        {
            return (PropertyInfo)((MemberExpression)expression.Body).Member;
        }

        protected IModelMetadataBuilder<TValue> Configure<TValue>(string propertyName)
        {
            var propertyInfo = ModelType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException($"Can't find the property by name:{propertyName}");
            }
            return GetOrCreateDisplayMetadataConfigurator<TValue>(propertyInfo);
        }

        private IModelMetadataBuilder<TValue> GetOrCreateDisplayMetadataConfigurator<TValue>(PropertyInfo propertyInfo)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var identity = ModelMetadataIdentity.ForProperty(propertyInfo.PropertyType, propertyInfo.Name, ModelType);
#pragma warning restore CS0618 // Type or member is obsolete
            IMetadataConfigurator configurator;
            if (!_configurators.TryGetValue(identity, out configurator))
            {
                configurator = new ModelMetadataBuilder<TValue>();
                _configurators.Add(identity, configurator);
            }
            return (IModelMetadataBuilder<TValue>)configurator;
        }

        public IDictionary<ModelMetadataIdentity, IMetadataConfigurator> MetadataConfigurators => new ReadOnlyDictionary<ModelMetadataIdentity, IMetadataConfigurator>(_configurators);
    }
}