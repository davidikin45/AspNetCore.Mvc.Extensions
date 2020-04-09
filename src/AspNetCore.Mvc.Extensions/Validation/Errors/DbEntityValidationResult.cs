using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Validation.Errors
{
    public class DbEntityValidationResult
    {
        public DbEntityValidationResult(IEnumerable<DbValidationError> validationErrors)
        {
            ValidationErrors = new List<DbValidationError>();
            foreach (var error in validationErrors)
            {
                ValidationErrors.Add(error);
            }
        }

        public DbEntityValidationResult()
        {
            ValidationErrors = new List<DbValidationError>();
        }

        public void AddModelError(string property, string errorMessage)
        {
            var error = new DbValidationError(property, errorMessage);
            ValidationErrors.Add(error);
        }

        public ICollection<DbValidationError> ValidationErrors { get; }
        public bool IsValid
        {
            get
            {
                return ValidationErrors.Count() == 0;
            }
        }
    }
}
