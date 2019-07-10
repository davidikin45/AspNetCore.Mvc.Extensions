using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxOrRadioTrueFalseAttribute : RadioOrCheckboxOptionsAttribute
    {
        public CheckboxOrRadioTrueFalseAttribute()
         : base(new List<string>() { "True", "False" }, new List<string>() { "true", "false" })
        {

        }
    }
}
