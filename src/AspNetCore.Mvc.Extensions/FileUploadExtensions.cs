using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions
{
    public static class FileUploadExtensions
    {
        public async static Task<(bool Success, string AbsoluteFilePath, string ContentRootRelativeFilePath, string ContentRootRelativeDirectoryPath, string DirectoryRelativeFilePath, string DirectoryRelativeDirectoryPath, string FileName)> SaveToContentDirectoryAsync(this IFormFile fileUpload, IHostingEnvironment environment, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        {
            var physicalFolder = environment.MapContentPath(directoryPath);

            if (!physicalFolder.EndsWith(@"\"))
            {
                physicalFolder = physicalFolder + @"\";
            }

            filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

            if(uniqueFileName)
            {
                filePath = Path.Combine(Path.GetDirectoryName(filePath), GetUniqueFileName(filePath));
            }

            string absolutePath = Path.Combine(physicalFolder, filePath);

            var result = await SaveAsAsync(fileUpload, absolutePath, createDirectoryIfNotExists);

            var contentRootRelativePath = result.AbsoluteFilePath.Replace(environment.ContentRootPath + @"\", "");
            var contentRootRelativeDirectoryPath = Path.GetDirectoryName(contentRootRelativePath);

            var directoryRelativePath = result.AbsoluteFilePath.Replace(physicalFolder, "");
            var directoryRelativeDirectoryPath = Path.GetDirectoryName(directoryRelativePath);

            var finalFilename = Path.GetFileName(absolutePath);

            if (result.Success)
                return (true, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename);
            else
                return (false, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename);
        }

        public async static Task<(bool Success, string AbsoluteFilePath, string ContentRootRelativeFilePath, string ContentRootRelativeDirectoryPath, string WebRootRelativeFilePath, string WebRootRelativeDirectoryPath, string DirectoryRelativeFilePath, string DirectoryRelativeDirectoryPath, string FileName, string VirtualPath)> SaveToWWwDirectoryAsync(this IFormFile fileUpload, IHostingEnvironment environment, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        {
            var physicalFolder = environment.MapWwwPath(directoryPath);

            if (!physicalFolder.EndsWith(@"\"))
            {
                physicalFolder = physicalFolder + @"\";
            }

            filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

            if (uniqueFileName)
            {
                filePath = Path.Combine(Path.GetDirectoryName(filePath), GetUniqueFileName(filePath));
            }

            string absolutePath = Path.Combine(physicalFolder, filePath);

            var result = await SaveAsAsync(fileUpload, absolutePath, createDirectoryIfNotExists);

            var webRootRelativePath = result.AbsoluteFilePath.Replace(environment.WebRootPath + @"\", "");
            var webRootRelativeDirectoryPath = Path.GetDirectoryName(webRootRelativePath);
            var virtualPath = "~/" + webRootRelativePath.Replace(@"\", @"/");

            var contentRootRelativePath = result.AbsoluteFilePath.Replace(environment.ContentRootPath + @"\", "");
            var contentRootRelativeDirectoryPath = Path.GetDirectoryName(contentRootRelativePath);

            var directoryRelativePath = result.AbsoluteFilePath.Replace(physicalFolder, "");
            var directoryRelativeDirectoryPath = Path.GetDirectoryName(directoryRelativePath);

            var finalFilename = Path.GetFileName(absolutePath);

            if (result.Success)
                return (true, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, webRootRelativePath, webRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename, virtualPath);
            else
                return (false, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, webRootRelativePath, webRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename, virtualPath);
        }

        public async static Task<(bool Success, string AbsoluteFilePath, string FileName)> SaveToDirectoryAsync(this IFormFile fileUpload, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        {
            filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

            if (uniqueFileName)
            {
                filePath = Path.Combine(Path.GetDirectoryName(filePath), GetUniqueFileName(filePath));
            }

            string absolutePath = Path.Combine(directoryPath, filePath);

            var result = await SaveAsAsync(fileUpload, absolutePath, createDirectoryIfNotExists);

            var finalFilename = Path.GetFileName(absolutePath);

            if (result.Success)
                return (true, result.AbsoluteFilePath, finalFilename);
            else
                return (false, result.AbsoluteFilePath, finalFilename);
        }

        public async static Task<(bool Success, string AbsoluteFilePath)> SaveAsAsync(this IFormFile fileUpload, string absoluteFilePath, bool createDirectoryIfNotExists = false)
        {
            if (fileUpload != null && fileUpload.Length > 0)
            {
                if(createDirectoryIfNotExists)
                    Directory.CreateDirectory(absoluteFilePath);

                using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }
                return (true, absoluteFilePath);
            }
            return (false, absoluteFilePath);
        }

        private static string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
    }
}
