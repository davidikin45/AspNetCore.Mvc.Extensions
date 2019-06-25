using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    //Composition Properties (1-To-Many, child cannot exist independent of the parent) 
    public class RepeaterAttribute : DataboundAttribute
    {
        public RepeaterAttribute(string displayExpression)
            : base(displayExpression)
        {

        }
    }

    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class FolderDropdownAttribute : DataboundAttribute
    {
        public FolderDropdownAttribute(string folderPath, Boolean nullable = false)
            : this(folderPath, nameof(DirectoryInfo.FullName), nameof(DirectoryInfo.LastWriteTime),"desc", nullable)
        {

        }

        public FolderDropdownAttribute(string folderPath, string displayExpression, string orderByProperty, string orderByType, Boolean nullable = false)
            : base(folderPath, null, displayExpression, orderByProperty, orderByType, nullable)
        {
        }
    }

    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class FileDropdownAttribute : DataboundAttribute
    {
        public FileDropdownAttribute(string folderPath, Boolean nullable = false)
            : this(folderPath, nameof(DirectoryInfo.Name), nameof(DirectoryInfo.LastWriteTime), "desc", nullable)
        {

        }

        public FileDropdownAttribute(string folderPath, string displayExpression, string orderByProperty, string orderByType, Boolean nullable = false)
            : base(null, folderPath, displayExpression, orderByProperty, orderByType, nullable)
        {
        }
    }

    //Aggregation relationshiships(child can exist independently of the parent, reference relationship)
    public class DropdownAttribute : DataboundAttribute
    {
        public DropdownAttribute(IEnumerable<string> options)
         : base("ModelDropdown", null, null, null, null, null, false, null, options)
        {

        }

        public DropdownAttribute(Type dropdownModelType, string displayExpression)
            : base("ModelDropdown", dropdownModelType, "Id", displayExpression, "Id", "desc", false, null)
        {

        }

        public DropdownAttribute(Type dropdownModelType, string displayExpression, string valueExpression)
        : base("ModelDropdown", dropdownModelType, valueExpression, displayExpression, "Id", "desc", false, null)
        {

        }

        public DropdownAttribute(Type dropdownModelType, string displayExpression, string orderByProperty, string orderByType)
           : base("ModelDropdown", dropdownModelType, "Id", displayExpression, orderByProperty, orderByType, false, null)
        {

        }

        public DropdownAttribute(Type dropdownModelType, string displayExpression, string orderByProperty, string orderByType, string bindingProperty)
          : base("ModelDropdown", dropdownModelType, "Id", displayExpression, orderByProperty, orderByType, false, bindingProperty)
        {

        }
    }

    public class YesNoCheckboxOrRadioAttribute : CheckboxOrRadioAttribute
    {
        public YesNoCheckboxOrRadioAttribute()
        : base(new List<string>() { "Yes", "No" })
        {

        }
    }

    public class TrueFalseCheckboxOrRadioAttribute : CheckboxOrRadioAttribute
    {
        public TrueFalseCheckboxOrRadioAttribute()
        : base(new List<string>() { "True", "False" })
        {

        }
    }

    public class CheckboxOrRadioAttribute : DataboundAttribute
    {
        public CheckboxOrRadioAttribute(IEnumerable<string> options)
           : base("ModelCheckboxOrRadio", null, null, null, null, null, false, null, options)
        {

        }

        public CheckboxOrRadioAttribute(Type selectorModelType, string displayExpression)
            : base("ModelCheckboxOrRadio", selectorModelType, "Id", displayExpression, "Id", "desc", false, null)
        {

        }

        public CheckboxOrRadioAttribute(Type selectorModelType, string displayExpression, string valueExpression)
        : base("ModelCheckboxOrRadio", selectorModelType, valueExpression, displayExpression, "Id", "desc", false, null)
        {

        }

        public CheckboxOrRadioAttribute(Type selectorModelType, string displayExpression, string orderByProperty, string orderByType)
           : base("ModelCheckboxOrRadio", selectorModelType, "Id", displayExpression, orderByProperty, orderByType, false, null)
        {

        }

        public CheckboxOrRadioAttribute(Type selectorModelType, string displayExpression, string valueExpression, string orderByProperty, string orderByType, string bindingProperty)
          : base("ModelCheckboxOrRadio", selectorModelType, valueExpression, displayExpression, orderByProperty, orderByType, false, bindingProperty)
        {

        }
    }

    public class YesNoCheckboxOrRadioButtonsAttribute : CheckboxOrRadioButtonsAttribute
    {
        public YesNoCheckboxOrRadioButtonsAttribute()
        : base(new List<string>() { "Yes", "No" })
        {

        }
    }

    public class TrueFalseCheckboxOrRadioButtonsAttribute : CheckboxOrRadioButtonsAttribute
    {
        public TrueFalseCheckboxOrRadioButtonsAttribute()
        : base(new List<string>() { "True", "False" })
        {

        }
    }

    public class CheckboxOrRadioButtonsAttribute : DataboundAttribute
    {
        public CheckboxOrRadioButtonsAttribute(IEnumerable<string> options)
           : base("ModelCheckboxOrRadioButtons", null, null, null, null, null, false, null, options)
        {

        }

        public CheckboxOrRadioButtonsAttribute(Type selectorModelType, string displayExpression)
            : base("ModelCheckboxOrRadioButtons", selectorModelType, "Id", displayExpression, "Id", "desc", false, null)
        {

        }

        public CheckboxOrRadioButtonsAttribute(Type selectorModelType, string displayExpression, string valueExpression)
        : base("ModelCheckboxOrRadioButtons", selectorModelType, valueExpression, displayExpression, "Id", "desc", false, null)
        {

        }

        public CheckboxOrRadioButtonsAttribute(Type selectorModelType, string displayExpression, string orderByProperty, string orderByType)
           : base("ModelCheckboxOrRadioButtons", selectorModelType, "Id", displayExpression, orderByProperty, orderByType, false, null)
        {

        }

        public CheckboxOrRadioButtonsAttribute(Type selectorModelType, string displayExpression, string valueExpression, string orderByProperty, string orderByType, string bindingProperty)
          : base("ModelCheckboxOrRadioButtons", selectorModelType, valueExpression, displayExpression, orderByProperty, orderByType, false, bindingProperty)
        {

        }
    }

    public class CheckboxOrRadioInlineAttribute : Attribute, IDisplayMetadataAttribute
    {
        public bool Inline { get; set; } = true;

        public void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["ModelCheckboxOrRadioInline"] = Inline;
        }
    }

    public abstract class DataboundAttribute : Attribute, IDisplayMetadataAttribute
    {
        public Type DropdownModelType { get; set; }
        public string KeyProperty { get; set; }
        public string BindingProperty { get; set; }
        public string DisplayExpression { get; set; }
        public string OrderByProperty { get; set; }
        public string OrderByTypeString { get; set; }

        public string PhysicalFolderPath { get; set; }

        public string PhysicalFilePath { get; set; }

        public Boolean Nullable { get; set; }

        public string DataTypeName { get; set; }

        public IEnumerable<string> Options { get; set; }

        public DataboundAttribute(string folderFolderPath, string fileFolderPath, string displayExpression, string orderByProperty, string orderByType, Boolean nullable = false)
        {

            PhysicalFolderPath = folderFolderPath;
            PhysicalFilePath = fileFolderPath;

            DataTypeName = "ModelDropdown";

            DisplayExpression = displayExpression;
            OrderByProperty = orderByProperty;
            OrderByTypeString = orderByType;

            Nullable = nullable;
        }

        //typeof
        //nameof
        public DataboundAttribute(string dataTypeName, Type dropdownModelType, string keyProperty, string displayExpression, string orderByProperty, string orderByType, Boolean nullable = false, string bindingProperty = null, IEnumerable<string> options = null)
        {
            DataTypeName = dataTypeName;

            DropdownModelType = dropdownModelType;
            KeyProperty = keyProperty;
            DisplayExpression = displayExpression;

            BindingProperty = bindingProperty;

            OrderByProperty = orderByProperty;
            OrderByTypeString = orderByType;
            Nullable = nullable;

            Options = options;
        }

        public DataboundAttribute(string displayExpression)
        {
            DataTypeName = "ModelRepeater";

            DisplayExpression = displayExpression;

            KeyProperty = "Id";
            BindingProperty = "Id";
        }

        public virtual void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.DataTypeName = DataTypeName;

            modelMetadata.AdditionalValues["IsDatabound"] = true;

            //Select from Db
            modelMetadata.AdditionalValues["DropdownModelType"] = DropdownModelType;
            modelMetadata.AdditionalValues["OrderByProperty"] = OrderByProperty;
            modelMetadata.AdditionalValues["OrderByType"] = OrderByTypeString;

            modelMetadata.AdditionalValues["KeyProperty"] = KeyProperty; //Used for dropdown 
            modelMetadata.AdditionalValues["DisplayExpression"] = DisplayExpression; //Used for Dropdown and Display Text

            modelMetadata.AdditionalValues["BindingProperty"] = BindingProperty;

            modelMetadata.AdditionalValues["Nullable"] = Nullable;

            var hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();

            modelMetadata.AdditionalValues["PhysicalFolderPath"] = hostingEnvironment.MapWwwPath(PhysicalFolderPath);
            modelMetadata.AdditionalValues["PhysicalFilePath"] = hostingEnvironment.MapWwwPath(PhysicalFilePath);

            modelMetadata.AdditionalValues["Options"] = Options;
        }
    }

}