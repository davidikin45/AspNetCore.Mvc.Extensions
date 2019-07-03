using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class EnumHelper
    {
        public static IList<SelectListItem> GetSelectList(Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata, Enum value)
        {
            if (metadata == null)
            {
                throw new NullReferenceException();
            }

            if (metadata.ModelType == null)
            {
                throw new Exception("Invalid model type");
            }

            if (!IsValidForEnumHelper(metadata))
            {
                throw new Exception("Invalid type");
            }

            return GetSelectList(metadata.ModelType, value);
        }


        public static IList<SelectListItem> GetSelectList(Type type, Enum value)
        {
            IList<SelectListItem> selectList = GetSelectList(type);

            Type valueType = (value == null) ? null : value.GetType();
            if (valueType != null && valueType != type && valueType != Nullable.GetUnderlyingType(type))
            {
                throw new Exception("Invalid type");
            }

            if (value == null && selectList.Count != 0 && String.IsNullOrEmpty(selectList[0].Value))
            {
                // Type is Nullable<T>; use existing entry to round trip null value
                selectList[0].Selected = true;
            }
            else
            {
                // If null, use default for this (non-Nullable<T>) enum -- always has 0 integral value
                string valueString = (value == null) ? "0" : value.ToString("d");

                // Select the last matching item, imitating what at least IE and Chrome highlight when multiple
                // elements in a <select/> element have a selected attribute.
                bool foundSelected = false;
                for (int i = selectList.Count - 1; !foundSelected && i >= 0; --i)
                {
                    SelectListItem item = selectList[i];
                    item.Selected = (valueString == item.Value);
                    foundSelected |= item.Selected;
                }

                // Round trip the current value
                if (!foundSelected)
                {
                    if (selectList.Count != 0 && String.IsNullOrEmpty(selectList[0].Value))
                    {
                        // Type is Nullable<T>; use existing entry for round trip
                        selectList[0].Selected = true;
                        selectList[0].Value = valueString;
                    }
                    else
                    {
                        // Add new entry which does not display value to user
                        selectList.Insert(0,
                            new SelectListItem { Selected = true, Text = String.Empty, Value = valueString, });
                    }
                }
            }

            return selectList;
        }

        public static IList<SelectListItem> GetSelectList(Type type)
        {
            if (type == null)
            {
                throw new NullReferenceException();
            }

            if (!IsValidForEnumHelper(type))
            {
                throw new Exception("Invalid type");
            }

            IList<SelectListItem> selectList = new List<SelectListItem>();

            // According to HTML5: "The first child option element of a select element with a required attribute and
            // without a multiple attribute, and whose size is "1", must have either an empty value attribute, or must
            // have no text content."  SelectExtensions.DropDownList[For]() methods often generate a matching
            // <select/>.  Empty value for Nullable<T>, empty text for round-tripping an unrecognized value, or option
            // label serves in some cases.  But otherwise, ignoring this does not cause problems in either IE or Chrome.
            Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
            if (checkedType != type)
            {
                // Underlying type was non-null so handle Nullable<T>; ensure returned list has a spot for null
                selectList.Add(new SelectListItem { Text = String.Empty, Value = String.Empty, });
            }

            // Populate the list
            const BindingFlags BindingFlags =
                BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static;
            foreach (FieldInfo field in checkedType.GetFields(BindingFlags))
            {
                // fieldValue will be an numeric type (byte, ...)
                object fieldValue = field.GetRawConstantValue();

                selectList.Add(new SelectListItem { Text = GetDisplayName(field), Value = fieldValue.ToString(), });
            }

            return selectList;
        }

        private static string GetDisplayName(FieldInfo field)
        {
            DisplayAttribute display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
            if (display != null)
            {
                string name = display.GetName();
                if (!String.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            return field.Name;
        }

        public static bool IsValidForEnumHelper(Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata)
        {
            return metadata != null && IsValidForEnumHelper(metadata.ModelType);
        }

        public static bool IsValidForEnumHelper(Type type)
        {
            bool isValid = false;
            if (type != null)
            {
                // Type.IsEnum is false for Nullable<T> even if T is an enum.  Check underlying type (if any).
                // Do not support Enum type itself -- IsEnum property is false for that class.
                Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
                if (checkedType.IsEnum)
                {
                    isValid = !HasFlagsInternal(checkedType);
                }
            }

            return isValid;
        }

        private static bool HasFlagsInternal(Type type)
        {
            FlagsAttribute attribute = type.GetCustomAttribute<FlagsAttribute>(inherit: false);
            return attribute != null;
        }
    }
}
