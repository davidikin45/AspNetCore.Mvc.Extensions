using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxOrRadioYesNoAttribute : CheckboxOrRadioOptionsAttribute
    {
        public CheckboxOrRadioYesNoAttribute()
         : base(new List<string>() { "Yes", "No" })
        {

        }
    }
}
