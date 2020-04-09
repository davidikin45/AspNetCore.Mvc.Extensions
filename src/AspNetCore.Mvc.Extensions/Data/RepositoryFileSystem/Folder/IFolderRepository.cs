namespace AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.Folder
{
    public interface IFolderRepository : IFolderReadOnlyRepository
    {
        void Create(string path);

        void Move(string sourcePath, string destinationPath);

        void Rename(string sourcePath, string newName);

        void Delete(string path);
    }
}
