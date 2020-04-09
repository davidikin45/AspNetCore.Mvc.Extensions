using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.File;
using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.Folder;
using System;
using System.Threading;

namespace AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem
{
    public interface IFileSystemGenericRepositoryFactory
    {
        IFileRepository CreateFileRepository(CancellationToken cancellationToken, string physicalPath, Boolean includeSubDirectories = false, string searchPattern = "*.*", params string[] extensions);
        IFileReadOnlyRepository CreateFileRepositoryReadOnly(CancellationToken cancellationToken, string physicalPath, Boolean includeSubDirectories = false, string searchPattern = "*.*", params string[] extensions);

        IImageInfoRepository CreateImageInfoRepository(CancellationToken cancellationToken, string physicalPath, Boolean includeSubDirectories = false);
        IImageInfoReadOnlyRepository CreateImageInfoRepositoryReadOnly(CancellationToken cancellationToken, string physicalPath, Boolean includeSubDirectories = false);

        IFolderRepository CreateFolderRepository(CancellationToken cancellationToken, string physicalPath, Boolean includeSubDirectories = false, string searchPattern = "*");
        IFolderReadOnlyRepository CreateFolderRepositoryReadOnly(CancellationToken cancellationToken, string physicalPath, Boolean includeSubDirectories = false, string searchPattern = "*");
    }
}
