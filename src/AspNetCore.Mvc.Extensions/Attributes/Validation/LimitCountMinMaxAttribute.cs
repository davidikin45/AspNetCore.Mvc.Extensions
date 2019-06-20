using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace DynamicForms.Attributes.Validation
{
    public class LimitCountMinMaxAttribute : ValidationAttribute
    {
        private readonly int _min;
        private readonly int _max;

        public LimitCountMinMaxAttribute(int min, int max)
        {
            _min = min;
            _max = max;
            if (_min == _max)
            {
                this.ErrorMessage = "Must select {1} options.";
            }
            else
            {
                this.ErrorMessage = "Must select between {1} and {2} options.";
            }
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;

            if (list == null && _min == 0)
                return true;

            if (list == null)
                return false;

            if (list.Count < _min || list.Count > _max)
                return false;

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(this.ErrorMessageString, name, _min, _max);
        }
    }
}
