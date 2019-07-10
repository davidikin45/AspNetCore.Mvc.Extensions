using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxTrueButtonAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public CheckboxTrueButtonAttribute()
         : base(new List<string>() { "True" }, new List<string>() { "true"})
        {
            Checkbox = true;
        }
    }
}
