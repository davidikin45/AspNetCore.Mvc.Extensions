using AspNetCore.Mvc.Extensions.RepositoryFileSystem.File;
using AspNetCore.Mvc.Extensions.RepositoryFileSystem.Folder;
using System;
using System.Threading;

namespace AspNetCore.Base.Data.RepositoryFileSystem
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
