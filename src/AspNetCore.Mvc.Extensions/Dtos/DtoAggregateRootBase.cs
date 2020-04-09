using AspNetCore.Mvc.Extensions.Attributes.Display;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public abstract class DtoAggregateRootBase<T> : DtoBase<T>, IDtoConcurrencyAware
    {
        //Optimistic Concurrency
        [HiddenInput, Render(ShowForCreate = false, ShowForDisplay = false, ShowForEdit = true, ShowForGrid = false)]
        public virtual byte[] RowVersion { get; set; }
    }
}
