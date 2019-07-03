using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxOrRadioButtonsTrueFalseAttribute : CheckboxOrRadioButtonsOptionsAttribute
    {
        public CheckboxOrRadioButtonsTrueFalseAttribute()
         : base(new List<string>() { "True", "False" })
        {

        }
    }
}
