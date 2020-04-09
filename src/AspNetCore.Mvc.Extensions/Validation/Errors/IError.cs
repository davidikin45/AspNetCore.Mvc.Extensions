namespace AspNetCore.Mvc.Extensions.Validation.Errors
{
    public interface IError
    {
        string PropertyName { get; }
        string PropertyExceptionMessage { get; }
    }
}
