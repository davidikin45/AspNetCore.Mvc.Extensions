﻿# ASP.NET Core MVC Extensions

```
services.AddMvcDisplayConventions(
new AppendAsterixToRequiredFieldLabels(),
new HtmlByNameConventionFilter(), 
new LabelTextConventionFilter(), 
new TextAreaByNameConventionFilter(), 
new TextboxPlaceholderConventionFilter(),
new DisableConvertEmptyStringToNull());

services.AddMvcValidationConventions();

services.AddMvcDisplayAttributes();

services.AddInheritanceValidationAttributeAdapterProvider();

services.AddFluentMetadata();
```

## Display Conventions
* IDisplayConventionFilter classes transform the display metadata by convention.
```
services.AddMvcDisplayConventions(
new AppendAsterixToRequiredFieldLabels(),
new HtmlByNameConventionFilter(), 
new LabelTextConventionFilter(), 
new TextAreaByNameConventionFilter(), 
new TextboxPlaceholderConventionFilter(),
new DisableConvertEmptyStringToNull());
```

| Display Convention                 | Description                                                                                     |
|:-----------------------------------|:------------------------------------------------------------------------------------------------|
| AppendAsterixToRequiredFieldLabels | Appends \* to label automatically                                                          	   |
| HtmlByNameConventionFilter         | If the field name contains 'html' DataTypeName will be set to 'Html'                            |
| LabelTextConventionFilter          | If a display attribute name is not set on model property FirstName would become 'First Name'    |
| TextAreaByNameConventionFilter     | If the field name contains 'body' or 'comments' DataTypeName will be set to 'MultilineText'     |
| TextboxPlaceholderConventionFilter | If Display attribute prompt is not set on model property FirstName would become 'First Name...' |
| DisableConvertEmptyStringToNull    | By default MVC converts empty string to Null, this disables the convention                      |


## Validation Conventions
* IValidationConventionFilter classes transform the validation metadata by convention.
```
services.AddMvcValidationConventions();
```

## Display Attributes
* IDisplayMetadataAttribute classes are a good way to define new editor/display types or pass additional information for existing via AdditionalValues.
```
services.AddMvcDisplayAttributes();
```

* SliderAttribute
```
public class SliderAttribute : Attribute, IDisplayMetadataAttribute
{
	public int Min { get; set; } = 0;
	public int Max { get; set; } = 100;

	public SliderAttribute()
	{

	}

	public SliderAttribute(int min, int max)
	{
		Min = min;
		Max = max;
	}

	public void TransformMetadata(DisplayMetadataProviderContext context)
	{
		var propertyAttributes = context.Attributes;
		var modelMetadata = context.DisplayMetadata;
		var propertyName = context.Key.Name;

		modelMetadata.DataTypeName = "Slider";
		modelMetadata.AdditionalValues["Min"] = Min;
		modelMetadata.AdditionalValues["Max"] = Max;
	}
}
```

* View Model
```
public class ViewModel
{
	[Slider(0, 200)]
	public int Value {get; set;}
}
```

* Views\Shared\EditorTemplates\Slider.cshtml
```
@model Int32?
@Html.TextBox("", ViewData.TemplateInfo.FormattedModelValue,
        new { @class = "custom-range", @type = "range", min = (int)ViewData.ModelMetadata.AdditionalValues["Min"], max = (int)ViewData.ModelMetadata.AdditionalValues["Max"] })
```

* Views\Shared\DisplayTemplates\Slider.cshtml
```
@model int?
@{ 
    var value = Html.ViewData.TemplateInfo.FormattedModelValue;
    var displayValue = "";
    if (value != null)
    {
        displayValue = value.ToString();
    }
}
@Html.Raw(displayValue)
```

## Validation Attribute Inheritance
* By default the [ValidationAttributeAdapterProdiver](https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.DataAnnotations/src/ValidationAttributeAdapterProvider.cs) doesn't perform client side validation if you inherit from an existing validation attribute.
* The below service collection extensions overrides the existing IValidationAttributeAdapterProvider enabling client side validation for inherited types. 
```
services.AddInheritanceValidationAttributeAdapterProvider();
```

| Attribute                  |
|:---------------------------|
| RegularExpressionAttribute |
| MaxLengthAttribute         |
| RequiredAttribute          |
| CompareAttribute           |
| MinLengthAttribute         |
| CreditCardAttribute        |
| StringLengthAttribute      |
| RangeAttribute             |
| EmailAddressAttribute      |
| PhoneAttribute             |
| UrlAttribute               |
| FileExtensionsAttribute    |


* Example
```
public class NumberValidatorAttribute : RangeAttribute
{
	public NumberValidatorAttribute()
		:base(0, int.MaxValue)
	{
		//The field {0} must be between {1} and {2}.
		ErrorMessage = "The field {0} must be a number.";
	}
}
```

## Fluent Metadata
* Allows modelmetadata to be configured via fluent syntax rather than attributes.
```
services.AddFluentMetadata();
```

* Example
```
public class PersonConfig : ModelMetadataConfiguration<Person>
{
	public PersonConfig()
	{
		Configure(p => p.Name).Required();
		Configure<string>("Name").Required();
	}
}
```

## Authors

* **Dave Ikin** - [davidikin45](https://github.com/davidikin45)


## License

This project is licensed under the MIT License