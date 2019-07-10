using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxNoButtonAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public CheckboxNoButtonAttribute()
         : base(new List<string>() { "No" }, new List<string>() { "true"})
        {
            Checkbox = true;
        }
    }
}
