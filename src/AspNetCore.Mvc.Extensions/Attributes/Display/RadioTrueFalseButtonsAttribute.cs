using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class RadioTrueFalseButtonsAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public RadioTrueFalseButtonsAttribute()
         : base(new List<string>() { "True", "False" }, new List<string>() { "true", "false" })
        {

        }
    }
}
