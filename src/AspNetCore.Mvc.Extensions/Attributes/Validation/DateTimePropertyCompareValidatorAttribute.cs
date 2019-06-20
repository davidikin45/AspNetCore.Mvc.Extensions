using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Attributes.Validation
{
    public class DateTimePropertyCompareValidatorAttribute : ValidationAttribute
    {
        private DateTimeCompareTypeEnum _CompareType;
        private string _OtherPropertyName;

        public DateTimePropertyCompareValidatorAttribute(
            DateTimeCompareTypeEnum compareType,
            string otherPropertyName)
        {
            _CompareType = compareType;
            _OtherPropertyName = otherPropertyName;
        }
        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Value cannot be null.");
            }

            var valueAsString = value.ToString();

            if (String.IsNullOrWhiteSpace(valueAsString) == true)
            {
                return new ValidationResult("Value cannot be blank.");
            }

            DateTime valueAsDateTime;

            if (DateTime.TryParse(valueAsString, out valueAsDateTime) == false)
            {
                return new ValidationResult("Value is not a DateTime.");
            }

            object otherValue = null;

            PropertyInfo otherPropertyInfo =
                validationContext.ObjectType.GetProperty(_OtherPropertyName);

            if (otherPropertyInfo == null)
            {

                return new ValidationResult("Invalid property name for other property.");
            }
            else
            {
                otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);
            }

            if (otherValue == null)
            {
                return new ValidationResult("Other property value not specified.");
            }

            DateTime otherValueAsDateTime;

            if (DateTime.TryParse(otherValue.ToString(), out otherValueAsDateTime) == false)
            {
                return new ValidationResult("Other value is not a DateTime.");
            }

            if (_CompareType == DateTimeCompareTypeEnum.GreaterThan)
            {
                if (valueAsDateTime == default(DateTime) ||
                    valueAsDateTime > otherValueAsDateTime)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(
                        String.Format(
                            "{0} should be greater than {1}.",
                            validationContext.DisplayName,
                            _OtherPropertyName));
                }
            }
            else
            {
                if (otherValueAsDateTime == default(DateTime) ||
                    valueAsDateTime < otherValueAsDateTime)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(
                        String.Format(
                            "{0} should be less than {1}.",
                            validationContext.DisplayName,
                            _OtherPropertyName));
                }
            }
        }

    }

    public enum DateTimeCompareTypeEnum
    {
        GreaterThan,
        LessThan
    }
}
