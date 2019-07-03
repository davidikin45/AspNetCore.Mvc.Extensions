using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.RepositoryFileSystem.File
{
    public interface IImageInfoReadOnlyRepository : IFileReadOnlyRepository
    {

        IEnumerable<ImageInfo> MetadataGetAll(
            Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
            int? skip = null,
            int? take = null);

        Task<IEnumerable<ImageInfo>> MetadataGetAllAsync(
          Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
          int? skip = null,
          int? take = null);

        IEnumerable<ImageInfo> MetadataGet(
            Expression<Func<FileInfo, bool>> filter = null,
            Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
            int? skip = null,
            int? take = null)
            ;

        Task<IEnumerable<ImageInfo>> MetadataGetAsync(
           Expression<Func<FileInfo, bool>> filter = null,
           Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
           ;

        IEnumerable<ImageInfo> MetadataSearch(
           string search = "",
           Expression<Func<FileInfo, bool>> filter = null,
           Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
           ;

        Task<IEnumerable<ImageInfo>> MetadataSearchAsync(
             string search = "",
           Expression<Func<FileInfo, bool>> filter = null,
           Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
           ;

        ImageInfo MetadataGetOne(
            Expression<Func<FileInfo, bool>> filter = null)
            ;

        Task<ImageInfo> MetadataGetOneAsync(
         Expression<Func<FileInfo, bool>> filter = null)
         ;

        ImageInfo MetadataGetFirst(
            Expression<Func<FileInfo, bool>> filter = null,
            Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null)
            ;

        Task<ImageInfo> MetadataGetFirstAsync(
          Expression<Func<FileInfo, bool>> filter = null,
          Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null)
          ;

        ImageInfo MetadataGetByPath(string path)
            ;

        Task<ImageInfo> MetadataGetByPathAsync(string path)
           ;

        ImageInfo MetadataGetMain()
         ;

        Task<ImageInfo> MetadataGetMainAsync();

    }

}
