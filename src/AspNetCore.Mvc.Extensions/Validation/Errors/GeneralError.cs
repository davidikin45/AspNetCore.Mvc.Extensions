namespace AspNetCore.Mvc.Extensions.Validation.Errors
{
    public class GeneralError : IError
    {
        #region Implementation of IError

        public string PropertyName { get { return string.Empty; } }
        public string PropertyExceptionMessage { get; set; }

        public GeneralError(string errorMessage)
        {
            this.PropertyExceptionMessage = errorMessage;
        }

        #endregion
    }
}
