using System;

namespace AspNetCore.Mvc.Extensions.Validation.Errors
{
    public class BadRequestError : Exception
    {
        public BadRequestError(string message):base(message)
        {

        }
    }
}
