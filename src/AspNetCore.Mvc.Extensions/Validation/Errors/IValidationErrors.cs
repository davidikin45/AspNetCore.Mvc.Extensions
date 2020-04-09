using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Validation.Errors
{
    public interface IValidationErrors
    {
        List<IError> Errors { get; set; }
    }
}
