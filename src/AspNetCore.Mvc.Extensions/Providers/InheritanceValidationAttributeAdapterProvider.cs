using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Providers
{
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.DataAnnotations/src/ValidationAttributeAdapterProvider.cs
    public class InheritanceValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly ValidationAttributeAdapterProvider defaultClientModelValidatorProvider;
        public InheritanceValidationAttributeAdapterProvider()
        {
            defaultClientModelValidatorProvider = new ValidationAttributeAdapterProvider();
        }

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            IAttributeAdapter adapter;

            var type = attribute.GetType();

            if (attribute is RegularExpressionAttribute)
            {
                adapter = new RegularExpressionAttributeAdapter((RegularExpressionAttribute)attribute, stringLocalizer);
            }
            else if (attribute is MaxLengthAttribute)
            {
                adapter = new MaxLengthAttributeAdapter((MaxLengthAttribute)attribute, stringLocalizer);
            }
            else if (attribute is RequiredAttribute)
            {
                adapter = new RequiredAttributeAdapter((RequiredAttribute)attribute, stringLocalizer);
            }
            else if (attribute is CompareAttribute)
            {
                adapter = new CompareAttributeAdapter((CompareAttribute)attribute, stringLocalizer);
            }
            else if (attribute is MinLengthAttribute)
            {
                adapter = new MinLengthAttributeAdapter((MinLengthAttribute)attribute, stringLocalizer);
            }
            else if (attribute is CreditCardAttribute)
            {
                adapter = new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "data-val-creditcard", stringLocalizer);
            }
            else if (attribute is StringLengthAttribute)
            {
                adapter = new StringLengthAttributeAdapter((StringLengthAttribute)attribute, stringLocalizer);
            }
            else if (attribute is RangeAttribute)
            {
                adapter = new RangeAttributeAdapter((RangeAttribute)attribute, stringLocalizer);
            }
            else if (attribute is EmailAddressAttribute)
            {
                adapter = new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "data-val-email", stringLocalizer);
            }
            else if (attribute is PhoneAttribute)
            {
                adapter = new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "data-val-phone", stringLocalizer);
            }
            else if (attribute is UrlAttribute)
            {
                adapter = new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "data-val-url", stringLocalizer);
            }
            else if (attribute is FileExtensionsAttribute)
            {
                adapter = new FileExtensionsAttributeAdapter((FileExtensionsAttribute)attribute, stringLocalizer);
            }
            else
            {
                adapter = defaultClientModelValidatorProvider.GetAttributeAdapter(attribute, stringLocalizer);
            }

            return adapter;
        }
    }
}
