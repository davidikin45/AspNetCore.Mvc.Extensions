using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Solution.Base.Validation
{    public class FileSizeAttribute : ValidationAttribute
    {
        //https://www.talkingdotnet.com/how-to-increase-file-upload-size-asp-net-core/
        public decimal MaxMegabytes { get; set; } = 28.0m;

        public long MaxBytes {get { return Decimal.ToInt64(MaxMegabytes * 1024 * 1024);  } }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            if (value is IFormFile)
            {
                return ValidateSingleFile((IFormFile)value);
            }
            else if (value is IEnumerable<IFormFile>)
            {
                return ValidateMultipleFiles(value);
            }
            else
            {
                return new ValidationResult("The input type is not a file.");
            }
        }
        private ValidationResult ValidateSingleFile(IFormFile file)
        {
            if (!isFileAllowedSize(file))
            {
                return DisallowedFileSizeError(file);
            }
            return ValidationResult.Success;
        }
        private ValidationResult ValidateMultipleFiles(object value)
        {
            IEnumerable<IFormFile> files = (IEnumerable<IFormFile>)value;
            foreach (var file in files)
            {
                if (!isFileAllowedSize(file))
                {
                    return DisallowedFileSizeError(file);
                }
            }
            return ValidationResult.Success;
        }
        private bool isFileAllowedSize(IFormFile file)
        {
            long fileSize = file.Length;
            return MaxBytes == 0 || fileSize <= MaxBytes;
        }
        private ValidationResult DisallowedFileSizeError(IFormFile file)
        {
            string fileName = file.FileName;
            return new ValidationResult($"The file size should not exceed {MaxMegabytes} MB.");
        }
    }
}
