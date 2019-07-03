using AspNetCore.Mvc.Extensions.RepositoryFileSystem.File;
using AspNetCore.Mvc.Extensions.RepositoryFileSystem.Folder;
using System.Threading;

namespace AspNetCore.Base.Data.RepositoryFileSystem
{
    public class FileSystemGenericRepositoryFactory : IFileSystemGenericRepositoryFactory
    {
        public FileSystemGenericRepositoryFactory()
            :base()
        {

        }

        public IFileRepository CreateFileRepository(CancellationToken cancellationToken, string physicalPath, bool includeSubDirectories = false, string searchPattern = "*.*", params string[] extensions)
        {
            return new FileRepository(physicalPath, includeSubDirectories, searchPattern, cancellationToken, extensions);
        }

        public IFileReadOnlyRepository CreateFileRepositoryReadOnly(CancellationToken cancellationToken, string physicalPath, bool includeSubDirectories = false, string searchPattern = "*.*", params string[] extensions)
        {
            return new FileReadOnlyRepository(physicalPath, includeSubDirectories, searchPattern, cancellationToken, extensions);
        }

        public IFolderRepository CreateFolderRepository(CancellationToken cancellationToken, string physicalPath, bool includeSubDirectories = false, string searchPattern = "*")
        {
            return new FolderRepository(physicalPath, includeSubDirectories, searchPattern, cancellationToken);
        }

        public IFolderReadOnlyRepository CreateFolderRepositoryReadOnly(CancellationToken cancellationToken, string physicalPath, bool includeSubDirectories = false, string searchPattern = "*")
        {
            return new FolderReadOnlyRepository(physicalPath, includeSubDirectories, searchPattern, cancellationToken);
        }

        public IImageInfoReadOnlyRepository CreateImageInfoRepositoryReadOnly(CancellationToken cancellationToken, string physicalPath, bool includeSubDirectories = false)
        {
            return new ImageInfoReadOnlyRepository(physicalPath, includeSubDirectories, cancellationToken);
        }

        public IImageInfoRepository CreateImageInfoRepository(CancellationToken cancellationToken, string physicalPath, bool includeSubDirectories = false)
        {
            return new ImageInfoRepository(physicalPath, includeSubDirectories, cancellationToken);
        }
    }
}
