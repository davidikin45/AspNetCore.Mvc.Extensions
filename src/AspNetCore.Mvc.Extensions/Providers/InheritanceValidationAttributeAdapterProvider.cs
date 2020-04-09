using Microsoft.AspNetCore.Mvc.DataAnnotations;
#if NETCOREAPP2_2
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
#endif
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
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

            IAttributeAdapter adapter = null;

            var type = attribute.GetType();

#if NETCOREAPP2_2
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
                adapter = new Microsoft.AspNetCore.Mvc.DataAnnotations.Internal.RequiredAttributeAdapter((RequiredAttribute)attribute, stringLocalizer);
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
#else
            if (attribute is RegularExpressionAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.RegularExpressionAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (RegularExpressionAttribute)attribute, stringLocalizer);
            }
            else if (attribute is MaxLengthAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.MaxLengthAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (MaxLengthAttribute)attribute, stringLocalizer);
            }
            else if (attribute is RequiredAttribute)
            {
                adapter = new RequiredAttributeAdapter((RequiredAttribute)attribute, stringLocalizer);
            }
            else if (attribute is CompareAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.CompareAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (CompareAttribute)attribute, stringLocalizer);
            }
            else if (attribute is MinLengthAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.MinLengthAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (MinLengthAttribute)attribute, stringLocalizer);
            }
            else if (attribute is CreditCardAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.DataTypeAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (DataTypeAttribute)attribute, "data-val-creditcard", stringLocalizer);
            }
            else if (attribute is StringLengthAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.StringLengthAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (StringLengthAttribute)attribute, stringLocalizer);
            }
            else if (attribute is RangeAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.RangeAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (RangeAttribute)attribute, stringLocalizer);
            }
            else if (attribute is EmailAddressAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.DataTypeAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (DataTypeAttribute)attribute, "data-val-email", stringLocalizer);
            }
            else if (attribute is PhoneAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.DataTypeAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (DataTypeAttribute)attribute, "data-val-phone", stringLocalizer);
            }
            else if (attribute is UrlAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.DataTypeAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (DataTypeAttribute)attribute, "data-val-url", stringLocalizer);
            }
            else if (attribute is FileExtensionsAttribute)
            {
                adapter = (IAttributeAdapter)Activator.CreateInstance(Type.GetType("Microsoft.AspNetCore.Mvc.DataAnnotations.FileExtensionsAttributeAdapter, Microsoft.AspNetCore.Mvc.DataAnnotations"), (FileExtensionsAttribute)attribute, stringLocalizer);
            }
            else
            {
                adapter = defaultClientModelValidatorProvider.GetAttributeAdapter(attribute, stringLocalizer);
            }
#endif

            return adapter;
        }
    }
}
