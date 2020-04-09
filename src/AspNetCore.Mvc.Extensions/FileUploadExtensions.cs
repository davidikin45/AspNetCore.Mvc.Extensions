using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions
{
   //A single IFormFile.
   // Any of the following collections that represent several files:
   //IFormFileCollection
   //IEnumerable<IFormFile>
   //List<IFormFile>

    public static class FileUploadExtensions
    {
        public async static Task<(
            bool Success, 
            string AbsoluteFilePath, 
            string ContentRootRelativeFilePath, 
            string ContentRootRelativeDirectoryPath, 
            string DirectoryRelativeFilePath, 
            string DirectoryRelativeDirectoryPath, 
            string FileName, 
            string DisplayName, 
            string ContentType)> SaveToContentDirectoryAsync(this IFormFile fileUpload, IWebHostEnvironment environment, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        {
            var physicalFolder = environment.MapContentPath(directoryPath);

            if (!physicalFolder.EndsWith(@"\"))
            {
                physicalFolder = physicalFolder + @"\";
            }

            filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

            var displayFileName = WebUtility.HtmlEncode(Path.GetFileName(filePath));

            if (uniqueFileName)
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
                return (true, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename, displayFileName,  fileUpload.ContentType);
            else
                return (false, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename, displayFileName, fileUpload.ContentType);
        }

        //Note when using Path.Combine the DirectoryRelativeUrl should not start with / and should end with /
        //Path.Combine("~/", WebRootRelativeDirectoryUrl, fileName)
        //Path.Combine("~/", "folder", DirectoryRelativeDirectoryUrl, fileName)
        //OR VirtualPath
        //public async static Task<(
        //    bool Success, 
        //    string AbsoluteFilePath,
        //    string ContentRootRelativeFilePath, 
        //    string ContentRootRelativeDirectoryPath, 
        //    string WebRootRelativeFilePath, 
        //    string WebRootRelativeFileUrl, 
        //    string WebRootRelativeDirectoryPath, 
        //    string WebRootRelativeDirectoryUrl, 
        //    string DirectoryRelativeFilePath, 
        //    string DirectoryRelativeUrl, 
        //    string DirectoryRelativeDirectoryPath, 
        //    string DirectoryRelativeDirectoryUrl, 
        //    string FileName, 
        //    string DisplayName, 
        //    string ContentType, 
        //    string VirtualPath)> SaveToWWwDirectoryAsync(this IFormFile fileUpload, IWebHostEnvironment environment, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        //{
        //    var physicalFolder = environment.MapWwwPath(directoryPath);

        //    if (!physicalFolder.EndsWith(@"\"))
        //    {
        //        physicalFolder = physicalFolder + @"\";
        //    }

        //    filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

        //    var displayFileName = WebUtility.HtmlEncode(Path.GetFileName(filePath));

        //    if (uniqueFileName)
        //    {
        //        filePath = Path.Combine(Path.GetDirectoryName(filePath), GetUniqueFileName(filePath));
        //    }

        //    var absolutePath = Path.Combine(physicalFolder, filePath);

        //    var result = await SaveAsAsync(fileUpload, absolutePath, createDirectoryIfNotExists);

        //    var webRootRelativePath = result.AbsoluteFilePath.Replace(environment.WebRootPath + @"\", "");
        //    var webRootRelativeDirectoryPath = Path.GetDirectoryName(webRootRelativePath);
        //    var webRootRelativeUrl = webRootRelativePath.Replace(@"\", @"/");
        //    var webRootRelativeDirectoryUrl = webRootRelativeDirectoryPath.Replace(@"\", @"/");
        //    var virtualPath = "~/" + webRootRelativeUrl;

        //    var contentRootRelativePath = result.AbsoluteFilePath.Replace(environment.ContentRootPath + @"\", "");
        //    var contentRootRelativeDirectoryPath = Path.GetDirectoryName(contentRootRelativePath);

        //    var directoryRelativePath = result.AbsoluteFilePath.Replace(physicalFolder, "");
        //    var directoryRelativeDirectoryPath = Path.GetDirectoryName(directoryRelativePath);
        //    var directoryRelativeUrl = directoryRelativePath.Replace(@"\", @"/");
        //    var directoryRelativeDirectoryUrl = directoryRelativeDirectoryPath.Replace(@"\", @"/");

        //    var finalFilename = Path.GetFileName(absolutePath);

        //    if (result.Success)
        //        return (true, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, webRootRelativePath, webRootRelativeUrl, webRootRelativeDirectoryPath, webRootRelativeDirectoryUrl, directoryRelativePath, directoryRelativeUrl, directoryRelativeDirectoryPath, directoryRelativeDirectoryUrl, finalFilename, displayFileName, fileUpload.ContentType, virtualPath);
        //    else
        //        return (false, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, webRootRelativePath, webRootRelativeUrl, webRootRelativeDirectoryPath, webRootRelativeDirectoryUrl, directoryRelativePath, directoryRelativeUrl, directoryRelativeDirectoryPath, directoryRelativeDirectoryUrl, finalFilename, displayFileName, fileUpload.ContentType, virtualPath);
        //}

        //public async static Task<(
        //    bool Success,
        //    string AbsoluteFilePath, 
        //    string ContentRootRelativeFilePath, 
        //    string ContentRootRelativeDirectoryPath, 
        //    string DirectoryRelativeFilePath, 
        //    string DirectoryRelativeDirectoryPath, 
        //    string FileName, 
        //    string DisplayName, 
        //    string ContentType)> SaveToContentDirectoryAsync(this IFormFile fileUpload, IHostingEnvironment environment, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        //{
        //    var physicalFolder = environment.MapContentPath(directoryPath);

        //    if (!physicalFolder.EndsWith(@"\"))
        //    {
        //        physicalFolder = physicalFolder + @"\";
        //    }

        //    filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

        //    var displayFileName = WebUtility.HtmlEncode(Path.GetFileName(filePath));

        //    if (uniqueFileName)
        //    {
        //        filePath = Path.Combine(Path.GetDirectoryName(filePath), GetUniqueFileName(filePath));
        //    }

        //    string absolutePath = Path.Combine(physicalFolder, filePath);

        //    var result = await SaveAsAsync(fileUpload, absolutePath, createDirectoryIfNotExists);

        //    var contentRootRelativePath = result.AbsoluteFilePath.Replace(environment.ContentRootPath + @"\", "");
        //    var contentRootRelativeDirectoryPath = Path.GetDirectoryName(contentRootRelativePath);

        //    var directoryRelativePath = result.AbsoluteFilePath.Replace(physicalFolder, "");
        //    var directoryRelativeDirectoryPath = Path.GetDirectoryName(directoryRelativePath);

        //    var finalFilename = Path.GetFileName(absolutePath);

        //    if (result.Success)
        //        return (true, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename, displayFileName, fileUpload.ContentType);
        //    else
        //        return (false, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, directoryRelativePath, directoryRelativeDirectoryPath, finalFilename, displayFileName, fileUpload.ContentType);
        //}

        ////Note when using Path.Combine the DirectoryRelativeUrl should not start with / and should end with /
        ////Path.Combine("~/", WebRootRelativeDirectoryUrl, fileName)
        ////Path.Combine("~/", "folder", DirectoryRelativeDirectoryUrl, fileName)
        ////OR VirtualPath
        //public async static Task<(
        //    bool Success,
        //    string AbsoluteFilePath,
        //    string ContentRootRelativeFilePath,
        //    string ContentRootRelativeDirectoryPath,
        //    string WebRootRelativeFilePath,
        //    string WebRootRelativeFileUrl,
        //    string WebRootRelativeDirectoryPath,
        //    string WebRootRelativeDirectoryUrl,
        //    string DirectoryRelativeFilePath,
        //    string DirectoryRelativeUrl,
        //    string DirectoryRelativeDirectoryPath,
        //    string DirectoryRelativeDirectoryUrl,
        //    string FileName,
        //    string DisplayName,
        //    string ContentType,
        //    string VirtualPath)> SaveToWWwDirectoryAsync(this IFormFile fileUpload, IHostingEnvironment environment, string directoryPath, string filePath = null, bool uniqueFileName = false, bool createDirectoryIfNotExists = false)
        //{
        //    var physicalFolder = environment.MapWwwPath(directoryPath);

        //    if (!physicalFolder.EndsWith(@"\"))
        //    {
        //        physicalFolder = physicalFolder + @"\";
        //    }

        //    filePath = string.IsNullOrEmpty(filePath) ? Path.GetFileName(fileUpload.FileName) : filePath;

        //    var displayFileName = WebUtility.HtmlEncode(Path.GetFileName(filePath));

        //    if (uniqueFileName)
        //    {
        //        filePath = Path.Combine(Path.GetDirectoryName(filePath), GetUniqueFileName(filePath));
        //    }

        //    var absolutePath = Path.Combine(physicalFolder, filePath);

        //    var result = await SaveAsAsync(fileUpload, absolutePath, createDirectoryIfNotExists);

        //    var webRootRelativePath = result.AbsoluteFilePath.Replace(environment.WebRootPath + @"\", "");
        //    var webRootRelativeDirectoryPath = Path.GetDirectoryName(webRootRelativePath);
        //    var webRootRelativeUrl = webRootRelativePath.Replace(@"\", @"/");
        //    var webRootRelativeDirectoryUrl = webRootRelativeDirectoryPath.Replace(@"\", @"/");
        //    var virtualPath = $"~/{webRootRelativeUrl}";

        //    var contentRootRelativePath = result.AbsoluteFilePath.Replace(environment.ContentRootPath + @"\", "");
        //    var contentRootRelativeDirectoryPath = Path.GetDirectoryName(contentRootRelativePath);

        //    var directoryRelativePath = result.AbsoluteFilePath.Replace(physicalFolder, "");
        //    var directoryRelativeDirectoryPath = Path.GetDirectoryName(directoryRelativePath);
        //    var directoryRelativeUrl = directoryRelativePath.Replace(@"\", @"/");
        //    var directoryRelativeDirectoryUrl = directoryRelativeDirectoryPath.Replace(@"\", @"/");

        //    var finalFilename = Path.GetFileName(absolutePath);

        //    if (result.Success)
        //        return (true, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, webRootRelativePath, webRootRelativeUrl, webRootRelativeDirectoryPath, webRootRelativeDirectoryUrl, directoryRelativeUrl, directoryRelativePath, directoryRelativeDirectoryPath, directoryRelativeDirectoryUrl, finalFilename, displayFileName, fileUpload.ContentType, virtualPath);
        //    else
        //        return (false, result.AbsoluteFilePath, contentRootRelativePath, contentRootRelativeDirectoryPath, webRootRelativePath, webRootRelativeUrl, webRootRelativeDirectoryPath, webRootRelativeDirectoryUrl, directoryRelativeUrl, directoryRelativePath, directoryRelativeDirectoryPath, directoryRelativeDirectoryUrl, finalFilename, displayFileName, fileUpload.ContentType, virtualPath);
        //}

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
                      //+ Path.GetRandomFileName()
                      + Path.GetExtension(fileName);
        }
    }
}
