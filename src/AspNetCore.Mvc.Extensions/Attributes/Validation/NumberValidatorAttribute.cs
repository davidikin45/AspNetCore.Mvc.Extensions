using System.ComponentModel.DataAnnotations;

namespace DynamicForms.Attributes.Validation
{
    public class NumberValidatorAttribute : RangeAttribute
    {
        public NumberValidatorAttribute()
            :base(0, int.MaxValue)
        {
            //The field {0} must be between {1} and {2}.
            ErrorMessage = "The field {0} must be a number.";
        }
    }
}
