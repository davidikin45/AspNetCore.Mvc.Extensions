using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class RadioYesNoAttribute : RadioOrCheckboxOptionsAttribute
    {
        public RadioYesNoAttribute()
         : base(new List<string>() { "Yes", "No" }, new List<string>() { "true", "false" })
        {

        }
    }
}
