using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxTextButtonAttribute : RadioOrCheckboxButtonsOptionsAttribute
    {
        public CheckboxTextButtonAttribute(string text)
         : base(new List<string>() { text }, new List<string>() { "true"})
        {
            Checkbox = true;
        }
    }
}
