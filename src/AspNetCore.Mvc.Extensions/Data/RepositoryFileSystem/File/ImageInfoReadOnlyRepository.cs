using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DND.Common.Implementation.Repository.FileSystem.File
{
    public class ImageInfoReadOnlyRepository : FileReadOnlyRepository, IImageInfoReadOnlyRepository
    {

        public ImageInfoReadOnlyRepository(string physicalPath, Boolean includeSubDirectories, CancellationToken cancellationToken = default(CancellationToken))
           : base(physicalPath, includeSubDirectories, "*.*",  cancellationToken, ".jpg", ".jpeg")
        {
           
        }

        public ImageInfo MapFileInfoToImageInfo(FileInfo fileInfo)
        {
            var metadata = new ImageInfo(fileInfo.FullName, _physicalPath);
            return metadata;
        }

        public IEnumerable<ImageInfo> MetadataGetAll(
          Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
          int? skip = null,
          int? take = null)
        {
            return base.GetAll(orderBy, skip, take).Select(s => MapFileInfoToImageInfo(s));
        }

        public async virtual Task<IEnumerable<ImageInfo>> MetadataGetAllAsync(
         Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
         int? skip = null,
         int? take = null)
        {
            return (await base.GetAllAsync(orderBy, skip, take)).Select(s => MapFileInfoToImageInfo(s));
        }

        public virtual IEnumerable<ImageInfo> MetadataGet(
            Expression<Func<FileInfo, bool>> filter = null,
            Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
            int? skip = null,
            int? take = null)
        {
            return base.Get(filter, orderBy, skip, take).Select(s => MapFileInfoToImageInfo(s));
        }

        public async virtual Task<IEnumerable<ImageInfo>> MetadataGetAsync(
           Expression<Func<FileInfo, bool>> filter = null,
           Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
        {
            return (await base.GetAsync(filter, orderBy, skip, take)).Select(s => MapFileInfoToImageInfo(s));
        }

        public virtual IEnumerable<ImageInfo> MetadataSearch(
          string search = "",
          Expression<Func<FileInfo, bool>> filter = null,
          Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
          int? skip = null,
          int? take = null)
        {
            return base.Search(search, filter, orderBy, skip, take).Select(s => MapFileInfoToImageInfo(s));
        }

        public async virtual Task<IEnumerable<ImageInfo>> MetadataSearchAsync(
           string search = "",
           Expression<Func<FileInfo, bool>> filter = null,
           Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null,
           int? skip = null,
           int? take = null)
        {
            return (await base.SearchAsync(search, filter, orderBy, skip, take)).Select(s => MapFileInfoToImageInfo(s));
        }

        public virtual ImageInfo MetadataGetOne(
            Expression<Func<FileInfo, bool>> filter = null)
        {
            return MapFileInfoToImageInfo(base.GetOne(filter));
        }

        public async virtual Task<ImageInfo> MetadataGetOneAsync(
           Expression<Func<FileInfo, bool>> filter = null)
        {
            return MapFileInfoToImageInfo(await base.GetOneAsync(filter));
        }

        public virtual ImageInfo MetadataGetMain()
        {
            return MapFileInfoToImageInfo(base.GetMain());
        }

        public async virtual Task<ImageInfo> MetadataGetMainAsync()
        {
            return MapFileInfoToImageInfo(await base.GetMainAsync());
        }

        public virtual ImageInfo MetadataGetFirst(
           Expression<Func<FileInfo, bool>> filter = null,
           Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null)
        {
            return MapFileInfoToImageInfo(base.GetFirst(filter,orderBy));
        }

        public async virtual Task<ImageInfo> MetadataGetFirstAsync(
          Expression<Func<FileInfo, bool>> filter = null,
          Func<IQueryable<FileInfo>, IOrderedQueryable<FileInfo>> orderBy = null)
        {
            return MapFileInfoToImageInfo(await base.GetFirstAsync(filter, orderBy));
        }

        public virtual ImageInfo MetadataGetByPath(string path)
        {
            return MapFileInfoToImageInfo(base.GetByPath(path));
        }

        public async virtual Task<ImageInfo> MetadataGetByPathAsync(string path)
        {
            return MapFileInfoToImageInfo(await base.GetByPathAsync(path));
        }
    }
}
