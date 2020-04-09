using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.Folder
{
    public interface IFolderReadOnlyRepository
    {

        IEnumerable<DirectoryInfo> GetAll(
            Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null,
            int? skip = null,
            int? take = null);

        Task<IEnumerable<DirectoryInfo>> GetAllAsync(
          Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null,
          int? skip = null,
          int? take = null);

        IEnumerable<DirectoryInfo> Get(
            Expression<Func<DirectoryInfo, bool>> filter = null,
            Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null,
            int? skip = null,
            int? take = null)
            ;

        Task<IEnumerable<DirectoryInfo>> GetAsync(
           Expression<Func<DirectoryInfo, bool>> filter = null,
           Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
           ;

        IEnumerable<DirectoryInfo> Search(
            string search = "",
           Expression<Func<DirectoryInfo, bool>> filter = null,
           Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
           ;

        Task<IEnumerable<DirectoryInfo>> SearchAsync(
             string search = "",
           Expression<Func<DirectoryInfo, bool>> filter = null,
           Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
           ;

        DirectoryInfo GetOne(
            Expression<Func<DirectoryInfo, bool>> filter = null)
            ;

        Task<DirectoryInfo> GetOneAsync(
         Expression<Func<DirectoryInfo, bool>> filter = null)
         ;

        DirectoryInfo GetFirst(
            Expression<Func<DirectoryInfo, bool>> filter = null,
            Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null)
            ;

        Task<DirectoryInfo> GetFirstAsync(
          Expression<Func<DirectoryInfo, bool>> filter = null,
          Func<IQueryable<DirectoryInfo>, IOrderedQueryable<DirectoryInfo>> orderBy = null)
          ;

        DirectoryInfo GetByPath(string path)
            ;

        Task<DirectoryInfo> GetByPathAsync(string path)
           ;

        int GetCount(Expression<Func<DirectoryInfo, bool>> filter = null)
            ;

        Task<int> GetCountAsync(Expression<Func<DirectoryInfo, bool>> filter = null)
           ;

        int GetSearchCount(string search = "", Expression<Func<DirectoryInfo, bool>> filter = null)
          ;

        Task<int> GetSearchCountAsync(string search = "", Expression<Func<DirectoryInfo, bool>> filter = null)
           ;

        bool GetExists(Expression<Func<DirectoryInfo, bool>> filter = null)
            ;

        Task<bool> GetExistsAsync(Expression<Func<DirectoryInfo, bool>> filter = null)
           ;

    }
}
