using System;
using System.IO;
using System.Threading;

namespace AspNetCore.Mvc.Extensions.RepositoryFileSystem.Folder

// Setting state manually is important in case of detached entities (entities loaded without change tracking or created outside of the current context).
{
    public class FolderRepository : FolderReadOnlyRepository, IFolderRepository
    {
        public FolderRepository(string physicalPath, Boolean includeSubDirectories, string searchPattern = "*", CancellationToken cancellationToken = default(CancellationToken))
            : base(physicalPath, includeSubDirectories, searchPattern, cancellationToken)
        {
        }

        public void Delete(string path)
        {
            var folder = GetByPath(path);
            folder.Delete(true);
        }

        public void Create(string path)
        {
            string physicalPath = _physicalPath + path;
            Directory.CreateDirectory(physicalPath);
        }

        public void Move(string sourcePath, string destinationPath)
        {
            var folder = GetByPath(sourcePath);
            var destination = _physicalPath + destinationPath;
            folder.MoveTo(destinationPath);
        }

        public void Rename(string sourcePath, string newName)
        {
            var source = _physicalPath + sourcePath;
            var destinationPath = _physicalPath + newName;
            Directory.Move(source, destinationPath);
        }
    }

}
