using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Solution.Base.Validation
{
    //https://www.filesignatures.net/
    public class FileExtensionsAttribute : ValidationAttribute
    {
        public string[] FileExtensions { get; set; }

        public Dictionary<string, List<byte[]>> FileSignatures =
        new Dictionary<string, List<byte[]>>
        {
           { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
           { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
           { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
            { ".pdf", new List<byte[]>
                {
                    new byte[] { 0x25, 0x50, 0x44, 0x46 }
                }
            },
            { ".doc", new List<byte[]>
                {
                    new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }
                }
            },
            { ".docx", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 }
                }
            },
        };

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
            if (!isValidFileExtensions(file))
            {
                return DisallowedFileExtensionError(file);
            }
            else if (!isValidSignature(file))
            {
                return DisallowedFileSignatureError(file);
            }
            return ValidationResult.Success;
        }
        private ValidationResult ValidateMultipleFiles(object value)
        {
            IEnumerable<IFormFile> files = (IEnumerable<IFormFile>)value;
            foreach (var file in files)
            {
                if (!isValidFileExtensions(file))
                {
                    return DisallowedFileExtensionError(file);
                }
                else if(!isValidSignature(file))
                {
                    return DisallowedFileSignatureError(file);
                }
            }
            return ValidationResult.Success;
        }

        private bool isValidFileExtensions(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant(); ;
            if(FileExtensions.Length != 0 && (string.IsNullOrEmpty(ext) || !FileExtensions.Contains(ext)))
            {
                return false;
            }

            return true;
        }

        private ValidationResult DisallowedFileExtensionError(IFormFile file)
        {
            string fileName = file.FileName;
            return new ValidationResult($"Invalid file extension. Only the following extensions {String.Join(", ", FileExtensions)} are supported.");
        }

        private bool isValidSignature(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (FileSignatures.ContainsKey(ext))
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);

                    using (var reader = new BinaryReader(memoryStream))
                    {
                        var signatures = FileSignatures[ext];
                        var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                        return signatures.Any(signature =>
                            headerBytes.Take(signature.Length).SequenceEqual(signature));
                    }
                }
            }

            return true;
        }

        private ValidationResult DisallowedFileSignatureError(IFormFile file)
        {
            return new ValidationResult($"Invalid file signature. The file's signature doesn't match the file's extension.");
        }
    }
}

