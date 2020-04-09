using AspNetCore.Mvc.Extensions.Validation.Errors;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Validation
{
    public class Result
    {
        public byte[] NewRowVersion { get; }
        public IEnumerable<ValidationResult> ObjectValidationErrors { get; }
        public bool IsSuccess { get; }
        public ErrorType? ErrorType { get; private set; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, ErrorType? errorType, IEnumerable<ValidationResult> objectValidationErrors, byte[] newRowVersion)
        {
            if (isSuccess && errorType != null)
                throw new InvalidOperationException();
            if (!isSuccess && errorType == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            ErrorType = errorType;
            ObjectValidationErrors = objectValidationErrors;
            NewRowVersion = newRowVersion;
        }

        public static Result ObjectValidationFail(string errorMessage)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage));
            return ObjectValidationFail(list);
        }

        public static Result ObjectValidationFail(string errorMessage, IEnumerable<string> memberNames)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage, memberNames));
            return ObjectValidationFail(list);
        }

        public static Result<T> ObjectValidationFail<T>(string errorMessage)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage));
            return ObjectValidationFail<T>(list);
        }

        public static Result<T> ObjectValidationFail<T>(string errorMessage, IEnumerable<string> memberNames)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage, memberNames));
            return ObjectValidationFail<T>(list);
        }

        public static Result ObjectDoesNotExist()
        {
            return new Result(false, Validation.ErrorType.ObjectDoesNotExist, new List<ValidationResult>(), null);
        }

        public static Result<T> ObjectDoesNotExist<T>()
        {
            return new Result<T>(default(T), false, Validation.ErrorType.ObjectDoesNotExist, new List<ValidationResult>(), null);
        }

        public static Result ObjectValidationFail(ValidationResult objectValidationError)
        {
            return new Result(false, Validation.ErrorType.ObjectValidationFailed, new ValidationResult[] { objectValidationError }, null);
        }

        public static Result<T> ObjectValidationFail<T>(ValidationResult objectValidationError)
        {
            return new Result<T>(default(T), false, Validation.ErrorType.ObjectValidationFailed, new ValidationResult[] { objectValidationError }, null);
        }

        public static Result ObjectValidationFail(IEnumerable<ValidationResult> ObjectValidationErrors)
        {
            return new Result(false,Validation.ErrorType.ObjectValidationFailed, ObjectValidationErrors, null);
        }

        public static Result<T> ObjectValidationFail<T>(IEnumerable<ValidationResult> ObjectValidationErrors)
        {
            return new Result<T>(default(T), false, Validation.ErrorType.ObjectValidationFailed, ObjectValidationErrors, null);
        }

        public static Result ConcurrencyConflict(string errorMessage)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage));
            return ConcurrencyConflict(list, null);
        }

        public static Result ConcurrencyConflict(IEnumerable<ValidationResult> concurrencyConflictErrors, byte[] newRowVersion)
        {
            return new Result(false, Validation.ErrorType.ConcurrencyConflict, concurrencyConflictErrors, newRowVersion);
        }

        public static Result<T> ConcurrencyConflict<T>(string errorMessage)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage));
            return ConcurrencyConflict<T>(list, null);
        }

        public static Result<T> ConcurrencyConflict<T>(IEnumerable<ValidationResult> concurrencyConflictErrors, byte[] newRowVersion)
        {
            return new Result<T>(default(T), false, Validation.ErrorType.ConcurrencyConflict, concurrencyConflictErrors, newRowVersion);
        }

        public static Result Unauthorized(string errorMessage)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage));
            return Unauthorized(list);
        }

        public static Result Unauthorized(IEnumerable<ValidationResult> unauthorizedErrors)
        {
            return new Result(false, Validation.ErrorType.Unauthorized, unauthorizedErrors, null);
        }

        public static Result<T> Unauthorized<T>(string errorMessage)
        {
            var list = new List<ValidationResult>();
            list.Add(new ValidationResult(errorMessage));
            return Unauthorized<T>(list);
        }

        public static Result<T> Unauthorized<T>(IEnumerable<ValidationResult> unauthorizedErrors)
        {
            return new Result<T>(default(T), false, Validation.ErrorType.Unauthorized, unauthorizedErrors, null);
        }

        public static Result DatabaseErrors(IEnumerable<DbEntityValidationResult> errors)
        {
            var list = new List<ValidationResult>();

            foreach (var err in errors.SelectMany(dbEntityValidationResult => dbEntityValidationResult.ValidationErrors))
            {
                list.Add(new ValidationResult(err.ErrorMessage, new string[] { err.PropertyName }));
            }

            return new Result(false, Validation.ErrorType.DatabaseValidationFailed, list, null);
        }

        public static Result<T> DatabaseErrors<T>(IEnumerable<DbEntityValidationResult> errors)
        {
            var list = new List<ValidationResult>();

            foreach (var err in errors.SelectMany(dbEntityValidationResult => dbEntityValidationResult.ValidationErrors))
            {
                list.Add(new ValidationResult(err.ErrorMessage, new string[] { err.PropertyName }));
            }

            return new Result<T>(default(T), false, Validation.ErrorType.DatabaseValidationFailed, list, null);
        }

        public static Result<T> DatabaseErrors<T>(IEnumerable<ValidationResult> errors)
        {
            return new Result<T>(default(T), false, Validation.ErrorType.DatabaseValidationFailed, errors, null);
        }

        public static Result Fail(ErrorType errorType)
        {
            return new Result(false, errorType, new List<ValidationResult>(), null);
        }

        public static Result<T> Fail<T>(ErrorType errorType)
        {
            return new Result<T>(default(T), false, errorType, new List<ValidationResult>(), null);
        }

        public static Result Ok()
        {
            return new Result(true, null, new List<ValidationResult>(), null);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, null, new List<ValidationResult>(), null);
        }

        public static Result Combine(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Ok();
        }

        public static Result Combine(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Ok();
        }
    }


    public class Result<T> : Result
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

        protected internal Result(T value, bool isSuccess, ErrorType? errorType, IEnumerable<ValidationResult> ObjectValidationErrors, byte[] newRowVersion)
            : base(isSuccess, errorType, ObjectValidationErrors, newRowVersion)
        {
            _value = value;
        }
    }
}
