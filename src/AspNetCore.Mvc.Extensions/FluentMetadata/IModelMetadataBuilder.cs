using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    /// <summary>
    ///     Defines a contract to fluently configure metadata.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IModelMetadataBuilder<out TValue>
    {
        /// <summary>
        ///     Sets the Display name.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> DisplayName(string value);

        /// <summary>
        ///     Sets the Display name.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> DisplayName(Func<string> value);

        /// <summary>
        ///     Sets the short display name.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Placeholder(string value);

        /// <summary>
        ///     Sets the short display name.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Placeholder(Func<string> value);


        /// <summary>
        ///     Sets the Description.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Description(string value);

        /// <summary>
        ///     Sets the Description.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Description(Func<string> value);

        /// <summary>
        ///     Marks the value to render as hidden input element in edit mode.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> AsHidden();

        /// <summary>
        ///     Marks the value to render as hidden input element in edit mode.
        /// </summary>
        /// <param name="hideSurroundingHtml">Indicates whether the value will appear in display mode</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> AsHidden(bool hideSurroundingHtml);

        /// <summary>
        ///     Hides surrounding HTML.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> HideSurroundingHtml();


        /// <summary>
        ///     Hides the value in display mode.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> HideForDisplay();


        /// <summary>
        ///     Hides the value in edit mode.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> HideForEdit();

        /// <summary>
        ///     Hides the value in both display and edit mode.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Hide();

        /// <summary>
        ///     Sets the display text when the value is null.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> NullDisplayText(string value);


        /// <summary>
        ///     Sets the order
        /// </summary>
        /// <param name="value">The order</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Order(int value);

        /// <summary>
        ///     Sets the format in display mode.
        /// </summary>
        /// <param name="format">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> DisplayFormat(string format);


        /// <summary>
        ///     Sets the format in edit mode.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> EditFormat(string format);


        /// <summary>
        ///     Sets format for both display and edit mode.
        /// </summary>
        /// <param name="format">The value.</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Format(string format);


        IModelMetadataBuilder<TValue> DisableConvertEmptyStringToNull();

        IModelMetadataBuilder<TValue> DataType(DataType type);

        IModelMetadataBuilder<TValue> AllowHtml();

        #region Validation

        /// <summary>
        ///     Marks the value as read only.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> ReadOnly();


        /// <summary>
        ///     Sets the other property that the value must match.
        /// </summary>
        /// <param name="otherProperty">The other property</param>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Compare(string otherProperty);


        /// <summary>
        ///     Marks the value as required.
        /// </summary>
        /// <returns></returns>
        IModelMetadataBuilder<TValue> Required(bool allowEmptyStrings = false);


        IModelMetadataBuilder<TValue> MaxLength(int maxlength);


        IModelMetadataBuilder<TValue> MinLength(int minlenth);

        IModelMetadataBuilder<TValue> StringLength(int maximumLength, int? minimumLength = null);

        #endregion
    }
}