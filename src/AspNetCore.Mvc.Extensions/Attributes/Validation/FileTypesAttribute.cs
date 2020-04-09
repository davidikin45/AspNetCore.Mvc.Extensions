using AspNetCore.Mvc.MvcAsApi.Internal;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Solution.Base.Validation
{
    public class AudioTypeAttribute : FileTypesAttribute
    {
        public AudioTypeAttribute()
        {
            FileTypes = new string[] { "audio/*" };
        }
    }
    public class VideoTypeAttribute : FileTypesAttribute
    {
        public VideoTypeAttribute()
        {
            FileTypes = new string[] { "video/*" };
        }
    }

    public class ImageTypeAttribute : FileTypesAttribute
    {
        public ImageTypeAttribute()
        {
            FileTypes = new string[] { "image/*" };
        }
    }

    public class AudioVideoImageTypeAttribute : FileTypesAttribute
    {
        public AudioVideoImageTypeAttribute()
        {
            FileTypes = new string[] { "audio/*", "video/*", "image/*"};
        }
    }

    public class FileTypesAttribute : ValidationAttribute
    {
        public string[] FileTypes { get; set; } //image/*, video/*, audio/*

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null)
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
            if (!isFileAllowedType(file))
            {
                return DisallowedFileTypeError(file);
            }
            return ValidationResult.Success;
        }
        private ValidationResult ValidateMultipleFiles(object value)
        {
            IEnumerable<IFormFile> files = (IEnumerable<IFormFile>)value;
            foreach (var file in files)
            {
                if (!isFileAllowedType(file))
                {
                    return DisallowedFileTypeError(file);
                }
            }
            return ValidationResult.Success;
        }

        private bool isFileAllowedType(IFormFile file)
        {
            if (FileTypes.Length == 0)
                return true;

            string fileType = file.ContentType.ToLower();

            var parsedContentType = new MediaType(fileType);
            for (var i = 0; i < FileTypes.Length; i++)
            {
                // For supported media types that are not wildcard patterns, confirm that this formatter
                // supports a more specific media type than requested e.g. OK if "text/*" requested and
                // formatter supports "text/plain".
                // contentType is typically what we got in an Accept header.
                var supportedMediaType = new MediaType(FileTypes[i]);
                if (parsedContentType.IsSubsetOf(supportedMediaType))
                {
                    return true;
                }
            }

            return false;
        }

        private ValidationResult DisallowedFileTypeError(IFormFile file)
        {
            return new ValidationResult($"Invalid file type. Only the following types {String.Join(", ", FileTypes)} are supported.");
        }
    }
}

