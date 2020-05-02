using AspNetCore.Mvc.Extensions.Data.Helpers;
using AspNetCore.Specification;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs
{

    public abstract class GetByIdQuery<TResult> : IQuery<TResult>
    {
        public object Id { get; }

        public GetByIdQuery(object id)
        {
            Id = id;
        }
    }

    public abstract class GetListQuery<TItem> : IQuery<PagedList<TItem>>
    {
        public Expression<Func<TItem, bool>> Where { get; set; } = (x) => true;

        public IEnumerable<Expression<Func<TItem, Object>>> Includes { get; set; } = new List<Expression<Func<TItem, Object>>>();

        public string OrderBy { get; set; } //name desc

        public int? PageNo { get; set; } //1

        public int? PageSize { get; set; } //10

        public string User { get; set; }

        public string Search { get; set; }

        public int? Skip
        {
            get
            {
                if (PageNo.HasValue && PageSize.HasValue)
                {
                    return (PageNo.Value - 1) * PageSize.Value;
                }
                return null;
            }
        }

        public int? Take
        {
            get
            {
                return PageSize;
            }
        }
    }



    public interface IQuery<TResult>
    {
    }
}
