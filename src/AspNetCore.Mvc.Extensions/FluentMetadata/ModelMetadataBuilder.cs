using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public class ModelMetadataBuilder<TValue> : IMetadataConfigurator,
        IModelMetadataBuilder<TValue>
    {
        private readonly IList<Action<DisplayMetadata>> _displayActions;
        private readonly IList<Action<ValidationMetadata>> _validationActions;

        public ModelMetadataBuilder()
        {
            _displayActions = new List<Action<DisplayMetadata>>();
            _validationActions = new List<Action<ValidationMetadata>>();
        }


        public void Configure(DisplayMetadata metadata)
        {
            foreach (var action in _displayActions)
            {
                action(metadata);
            }
        }

        public void Configure(ValidationMetadata metadata)
        {
            foreach (var action in _validationActions)
            {
                action(metadata);
            }
        }

        protected IModelMetadataBuilder<TValue> AddDisplayMetadataAction(Action<DisplayMetadata> action)
        {
            _displayActions.Add(action);
            return this;
        }

        protected IModelMetadataBuilder<TValue> AddValidationMetadataAction(Action<ValidationMetadata> action)
        {
            _validationActions.Add(action);
            return this;
        }

        #region IModelMetadataItemBuilder

        public IModelMetadataBuilder<TValue> DisplayName(string value)
        {
            return DisplayName(() => value);
        }

        public IModelMetadataBuilder<TValue> DisplayName(Func<string> value)
        {
            return AddDisplayMetadataAction(m => m.DisplayName = value);
        }

        public IModelMetadataBuilder<TValue> Placeholder(string value)
        {
            return Placeholder(() => value);
        }

        public IModelMetadataBuilder<TValue> Placeholder(Func<string> value)
        {
            return AddDisplayMetadataAction(m => m.Placeholder = value);
        }

        public IModelMetadataBuilder<TValue> Description(string value)
        {
            return Description(() => value);
        }

        public IModelMetadataBuilder<TValue> Description(Func<string> value)
        {
            return AddDisplayMetadataAction(m => m.Description = value);
        }

        public IModelMetadataBuilder<TValue> AsHidden()
        {
            return AddDisplayMetadataAction(m => m.TemplateHint = "HiddenInput");
        }

        public IModelMetadataBuilder<TValue> AsHidden(bool hideSurroundingHtml)
        {
            return AddDisplayMetadataAction(m =>
            {
                m.TemplateHint = "HiddenInput";
                m.HideSurroundingHtml = hideSurroundingHtml;
            });
        }

        public IModelMetadataBuilder<TValue> HideSurroundingHtml()
        {
            return AddDisplayMetadataAction(m => m.HideSurroundingHtml = true);
        }

        public IModelMetadataBuilder<TValue> HideForDisplay()
        {
            return AddDisplayMetadataAction(m => m.ShowForDisplay = false);
        }

        public IModelMetadataBuilder<TValue> HideForEdit()
        {
            return AddDisplayMetadataAction(m => m.ShowForEdit = false);
        }

        public IModelMetadataBuilder<TValue> Hide()
        {
            return AddDisplayMetadataAction(m =>
            {
                m.ShowForEdit = false;
                m.ShowForDisplay = false;
            });
        }

        public IModelMetadataBuilder<TValue> NullDisplayText(string value)
        {
            return AddDisplayMetadataAction(m => m.NullDisplayText = value);
        }

        public IModelMetadataBuilder<TValue> Order(int value)
        {
            return AddDisplayMetadataAction(m => m.Order = value);
        }

        public IModelMetadataBuilder<TValue> DisplayFormat(string format)
        {
            return AddDisplayMetadataAction(m => m.DisplayFormatString = format);
        }

        public IModelMetadataBuilder<TValue> EditFormat(string format)
        {
            return AddDisplayMetadataAction(m => m.EditFormatString = format);
        }

        public IModelMetadataBuilder<TValue> Format(string format)
        {
            return AddDisplayMetadataAction(m =>
            {
                m.EditFormatString = format;
                m.DisplayFormatString = format;
            });
        }

        public IModelMetadataBuilder<TValue> AllowHtml()
        {
            return AddDisplayMetadataAction(m =>
            {
                m.HtmlEncode = false;
                m.DataTypeName = System.ComponentModel.DataAnnotations.DataType.Html.ToString();
            });
        }

        public IModelMetadataBuilder<TValue> DataType(DataType type)
        {
            return AddDisplayMetadataAction(m => m.DataTypeName = type.ToString());
        }

        public IModelMetadataBuilder<TValue> DisableConvertEmptyStringToNull()
        {
            return AddDisplayMetadataAction(m => m.ConvertEmptyStringToNull = false);
        }

        #region Validation

        public IModelMetadataBuilder<TValue> ReadOnly()
        {
            return AddValidationMetadataAction(v => { v.AddValidationAttribute(ReadOnlyAttribute.Yes); });
        }


        public IModelMetadataBuilder<TValue> Compare(string otherProperty)
        {
            return AddValidationMetadataAction(v => { v.AddValidationAttribute(new CompareAttribute(otherProperty)); });
        }


        public IModelMetadataBuilder<TValue> Required(bool allowEmptyStrings = false)
        {
            return AddValidationMetadataAction(v =>
            {
                v.IsRequired = true;
                v.AddValidationAttribute(new RequiredAttribute { AllowEmptyStrings = allowEmptyStrings });
            });
        }

        public IModelMetadataBuilder<TValue> MaxLength(int maxlength)
        {
            return AddValidationMetadataAction(v => { v.AddValidationAttribute(new MaxLengthAttribute(maxlength)); });
        }

        public IModelMetadataBuilder<TValue> MinLength(int minlenth)
        {
            return AddValidationMetadataAction(v => { v.AddValidationAttribute(new MinLengthAttribute(minlenth)); });
        }

        public IModelMetadataBuilder<TValue> StringLength(int maximumLength, int? minimumLength = null)
        {
            return AddValidationMetadataAction(v =>
            {
                var attribute = new StringLengthAttribute(maximumLength);
                if (minimumLength.HasValue)
                {
                    attribute.MinimumLength = minimumLength.Value;
                }
                v.AddValidationAttribute(attribute);
            });
        }

        #endregion

        #endregion
    }
}