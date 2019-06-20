using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicForms.Attributes.Validation
{
    public class AgeValidatorAttribute : ValidationAttribute
    {
        public int Age { get; set; }

        public AgeValidatorAttribute(int age)
        {
            Age = age;
            ErrorMessage = "Must be older than {1} years of age.";
        }

        public override bool IsValid(object value)
        {
            DateTime dob;
            if (value != null && DateTime.TryParse(value.ToString(), out dob))
            {
                if (dob.AddYears(Age) <= DateTime.Now)
                {
                    return true;
                }
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(this.ErrorMessageString, name, Age);
        }
    }
}
