using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class RadioYesNoButtonsAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public RadioYesNoButtonsAttribute()
         : base(new List<string>() { "Yes", "No" }, new List<string>() { "true", "false" })
        {

        }
    }
}
