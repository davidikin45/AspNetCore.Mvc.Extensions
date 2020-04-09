using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public class CountList<T> : List<T>
    {
        public int TotalCount { get; private set; }

        public CountList(List<T> items, int count)
        {
            TotalCount = count;
            AddRange(items);
        }

        public static CountList<T> Create(IQueryable<T> source, int? skip, int? take)
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

            return new CountList<T>(items, count);
        }

        public static async Task<CountList<T>> CreateAsync(IQueryable<T> source, int? skip, int? take, CancellationToken cancellationToken)
        {
            var countTask = source.CountAsync(cancellationToken).ConfigureAwait(false);
            var count = await countTask;

            if (skip.HasValue)
            {
                source = source.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                source = source.Take(take.Value);
            }

            var itemsTask = source.ToListAsync(cancellationToken).ConfigureAwait(false);

            var items = await itemsTask;

            return new CountList<T>(items, count);
        }
    }
}
