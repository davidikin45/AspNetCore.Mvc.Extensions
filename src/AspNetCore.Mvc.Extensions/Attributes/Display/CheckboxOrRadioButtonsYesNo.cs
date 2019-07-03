using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxOrRadioButtonsYesNo : CheckboxOrRadioButtonsOptionsAttribute
    {
        public CheckboxOrRadioButtonsYesNo()
         : base(new List<string>() { "Yes", "No" })
        {

        }
    }
}
