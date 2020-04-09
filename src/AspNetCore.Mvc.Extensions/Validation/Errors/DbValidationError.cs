using System;

namespace AspNetCore.Mvc.Extensions.Validation.Errors
{
    [Serializable]
    public class DbValidationError
    {
        private readonly string _propertyName;

        private readonly string _errorMessage;

        public DbValidationError(string propertyName, string errorMessage)
        {
            _propertyName = propertyName;
            _errorMessage = errorMessage;
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
    }
}
