using AspNetCore.Mvc.Extensions.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AspNetCore.Mvc.Extensions.Attributes.Validation
{
    //https://enterprisecraftsmanship.com/posts/combining-asp-net-core-attributes-with-value-objects/
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class ValueObjectAttribute : ValidationAttribute
    {
        private readonly Type _valueObjectType;
        public ValueObjectAttribute(Type type)
        {
            _valueObjectType = type;
        }

        protected override ValidationResult IsValid(
                object value, ValidationContext validationContext)
        {
            var propertyMap = new Dictionary<string, string>();

            var propertyType = validationContext.ObjectType.GetProperty(validationContext.MemberName)?.PropertyType ?? validationContext.ObjectType;

            var args = new List<object>();

            var isPropertyInValueTypeDto = true;
            var isPropertyValidation = true;
            ConstructorInfo constructor = null;

            if (propertyType.IsValueType || propertyType == typeof(string))
            {
                isPropertyValidation = true;

                var relatedProperties = validationContext.ObjectType.GetProperties().Where(p => p.CustomAttributes.Any(ca => ca.AttributeType == this.GetType())).ToList();
                constructor = _valueObjectType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, relatedProperties.Select(p => p.PropertyType).ToArray(), null);

                if (relatedProperties.Count > 1 && constructor != null)
                {
                    //from
                    var properties = relatedProperties.ToList();

                    //to
                    foreach (var param in constructor.GetParameters())
                    {
                        var props = properties.Where(p => p.PropertyType == param.ParameterType).ToList();
                        if (props.Count() > 1)
                        {
                            if (props.Any(p => string.Equals(p.Name, param.Name, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                props = props.Where(p => string.Equals(p.Name, param.Name, StringComparison.CurrentCultureIgnoreCase)).ToList();
                            }
                            else
                            {
                                props = props.GetRange(0, 1);
                            }
                        }

                        var prop = props.First();
                        propertyMap.Add(prop.Name, param.Name);

                        properties.Remove(prop);

                        var propValue = prop.GetValue(validationContext.ObjectInstance);
                        args.Add(propValue);
                    }
                }
                else
                {
                    constructor = _valueObjectType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { propertyType }, null);
                    propertyMap.Add(validationContext.MemberName, constructor.GetParameters()[0].Name);
                    ; isPropertyInValueTypeDto = false;
                    args.Add(value);
                }
            }
            else
            {

                if (value == null) //object
                    return ValidationResult.Success;

                isPropertyValidation = false;
                constructor = _valueObjectType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, propertyType.GetProperties().Select(p => p.PropertyType).ToArray(), null);

                var properties = propertyType.GetProperties().ToList();
                foreach (var param in constructor.GetParameters())
                {
                    var props = properties.Where(p => p.PropertyType == param.ParameterType).ToList();
                    if (props.Count() > 1)
                    {
                        if (props.Any(p => string.Equals(p.Name, param.Name, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            props = props.Where(p => string.Equals(p.Name, param.Name, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        }
                        else
                        {
                            props = props.GetRange(0, 1);
                        }
                    }

                    var prop = props.First();
                    propertyMap.Add(prop.Name, param.Name);

                    properties.Remove(prop);

                    var propValue = prop.GetValue(value);
                    args.Add(propValue);
                }
            }

            object valueObject = constructor.Invoke(args.ToArray());

            var errors = new List<ValidationResult>();

            if (isPropertyValidation)
            {
                var propertyName = propertyMap[validationContext.MemberName];
                if (isPropertyInValueTypeDto)
                {
                    errors = Validate(valueObject).Where(e => e.MemberNames.Contains(propertyName, StringComparer.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    errors = Validate(valueObject).Where(e => e.MemberNames.Contains(propertyName, StringComparer.OrdinalIgnoreCase) || e.MemberNames.Count() == 0).ToList();
                }

                errors.ForEach(e => e.ErrorMessage = Regex.Replace(e.ErrorMessage, propertyName, validationContext.DisplayName, RegexOptions.IgnoreCase));
            }
            else
            {
                errors = Validate(valueObject).Where(e => e.MemberNames.Count() == 0).ToList();
            }

            if (errors.Count > 0)
            {
                return new ValidationResult(errors.First().ErrorMessage);
            }

            return ValidationResult.Success;
        }

        //1. [Required]
        //2. Other attributes
        //3. IValidatableObject Implementation

        public IEnumerable<ValidationResult> Validate(object o)
        {
            var context = new ValidationContext(o);

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                context.ObjectInstance,
                context,
               validationResults,
               validateAllProperties: true); // if true [Required] + Other attributes

            return validationResults.Where(r => r != ValidationResult.Success);
        }
    }
}
