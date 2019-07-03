using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class CheckboxOrRadioTrueFalseAttribute : CheckboxOrRadioOptionsAttribute
    {
        public CheckboxOrRadioTrueFalseAttribute()
         : base(new List<string>() { "True", "False" })
        {

        }
    }
}
