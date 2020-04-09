using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPrevious => (CurrentPage > 1);
        public bool HasNext => (CurrentPage < TotalPages);

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static PagedList<T> Create(IQueryable<T> source, int? skip, int? take)
        {
            var count = source.Count();
            if (skip.HasValue)
            {
                source = source.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                source = source.Take(take.Value);
            }

            var items= source.ToList();

            var pageNumber = 1;
            if (skip.HasValue && take.HasValue && take.Value > 0)
            {
                pageNumber = (skip.Value / take.Value) + 1;
            }

            return new PagedList<T>(items, count, pageNumber, take ?? count);
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int? skip, int? take, CancellationToken cancellationToken)
        {
            var countTask = source.CountAsync(cancellationToken).ConfigureAwait(false);

            if(skip.HasValue)
            {
                source = source.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                source = source.Take(take.Value);
            }

            var itemsTask = source.ToListAsync(cancellationToken).ConfigureAwait(false);

            var count = await countTask;
            var items = await itemsTask;

            var pageNumber = 1;
            if(skip.HasValue && take.HasValue && take.Value > 0)
            {
                pageNumber = (skip.Value / take.Value) + 1;
            }

            return new PagedList<T>(items, count, pageNumber, take ?? count);
        }
    }
}
