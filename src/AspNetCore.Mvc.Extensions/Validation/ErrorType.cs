namespace AspNetCore.Mvc.Extensions.Validation
{
    public enum ErrorType
    {
        UnknownError,
        ObjectDoesNotExist,
        ObjectValidationFailed,
        ConcurrencyConflict,
        EmailSendFailed,
        Unauthorized,
        DatabaseValidationFailed
    }
}
