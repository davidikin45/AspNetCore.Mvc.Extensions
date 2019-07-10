using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxYesButtonAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public CheckboxYesButtonAttribute()
         : base(new List<string>() { "Yes" }, new List<string>() { "true"})
        {
            Checkbox = true;
        }
    }
}
