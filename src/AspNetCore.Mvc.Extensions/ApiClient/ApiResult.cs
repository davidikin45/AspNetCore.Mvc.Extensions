using Microsoft.AspNetCore.Http;
using System;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    public class ApiResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorResponse { get; }
        public int HttpStatusCode { get; }

        protected ApiResult(bool isSuccess, int httpStatusCode, string errorResponse)
        {
            if (isSuccess && errorResponse != null)
                throw new InvalidOperationException();
            if (!isSuccess && errorResponse == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            HttpStatusCode = httpStatusCode;

        }

        public static ApiResult Fail(int httpStatusCode, string errorResponse)
        {
            return new ApiResult(false, httpStatusCode, errorResponse);
        }

        public static ApiResult<T> Fail<T>(int httpStatusCode, string errorResponse)
        {
            return new ApiResult<T>(default(T), false, httpStatusCode, errorResponse);
        }

        public static ApiResult Ok()
        {
            return new ApiResult(true, StatusCodes.Status200OK, null);
        }

        public static ApiResult<T> Ok<T>(T value)
        {
            return new ApiResult<T>(value, true, StatusCodes.Status200OK, null);
        }

        public static ApiResult Combine(params ApiResult[] results)
        {
            foreach (ApiResult result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Ok();
        }
    }

    public class ApiResult<T> : ApiResult
    {
        private readonly T _value;
        public T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException();

                return _value;
            }
        }

        protected internal ApiResult(T value, bool isSuccess, int httpStatusCode, string errorResponse)
            : base(isSuccess, httpStatusCode, errorResponse)
        {
            _value = value;
        }
    }
}
