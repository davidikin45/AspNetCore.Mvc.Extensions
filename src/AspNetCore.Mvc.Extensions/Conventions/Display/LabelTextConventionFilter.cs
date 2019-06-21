using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspNetCore.Mvc.Extensions.Conventions.Display
{
    public class LabelTextConventionFilter : IDisplayConventionFilter
    {
        private readonly Func<DisplayMetadataProviderContext, bool> _applyConvention;
        public LabelTextConventionFilter()
            : this((context) => true)
        {

        }

        public LabelTextConventionFilter(Func<DisplayMetadataProviderContext, bool> applyConvention)
        {
            _applyConvention = applyConvention;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;


            if (IsTransformRequired(propertyName, modelMetadata, propertyAttributes) && _applyConvention(context))
            {

                modelMetadata.DisplayName = () => GetStringWithSpaces(propertyName);
            }
        }

        private bool IsTransformRequired(string propertyName, DisplayMetadata modelMetadata, IReadOnlyList<object> propertyAttributes)
        {
            if (!string.IsNullOrEmpty(modelMetadata.SimpleDisplayProperty))
                return false;

            if (propertyAttributes.OfType<DisplayNameAttribute>().Any())
                return false;

            if (propertyAttributes.OfType<DisplayAttribute>().Any())
                return false;

            if (string.IsNullOrEmpty(propertyName))
                return false;

            return true;
        }

        private string GetStringWithSpaces(string input)
        {
            return Regex.Replace(
               input,
               "(?<!^)" +
               "(" +
               "  [A-Z][a-z] |" +
               "  (?<=[a-z])[A-Z] |" +
               "  (?<![A-Z])[A-Z]$" +
               ")",
               " $1",
               RegexOptions.IgnorePatternWhitespace);
        }
    }
}