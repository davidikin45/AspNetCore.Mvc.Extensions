using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class ChecboxFalseButtonAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public ChecboxFalseButtonAttribute()
         : base(new List<string>() { "False" }, new List<string>() { "true"})
        {
            Checkbox = true;
        }
    }
}
