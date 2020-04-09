using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.File;
using System;
using System.IO;
using System.Threading;

namespace DND.Common.Implementation.Repository.FileSystem.File
{
    public class FileRepository : FileReadOnlyRepository, IFileRepository
    {
        public FileRepository(string physicalPath, Boolean includeSubDirectories, string searchPattern = "*.*", CancellationToken cancellationToken = default(CancellationToken), params string[] extensions)
            : base(physicalPath, includeSubDirectories, searchPattern, cancellationToken, extensions)
        {
        }

        public void Delete(string path)
        {
            var file = GetByPath(path);
            file.Delete();
        }

        public void Save(string path, byte[] bytes)
        {
            string physicalPath = _physicalPath + path;
            System.IO.File.WriteAllBytes(physicalPath, bytes);
        }

        public void Save(string path, string text)
        {
            string physicalPath = _physicalPath + path;
            System.IO.File.WriteAllText(physicalPath, text);
        }

        public void Move(string sourcePath, string destinationPath)
        {
            var file = GetByPath(sourcePath);
            var destination = _physicalPath + destinationPath;
            file.MoveTo(destinationPath);
        }

        public void Rename(string sourcePath, string newName)
        {
            var source = _physicalPath + sourcePath;
            var directory = Path.GetDirectoryName(source);
            var destinationPath = Path.Combine(directory, newName);
            System.IO.File.Move(source, destinationPath);
        }
    }

}
