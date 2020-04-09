using System;

namespace AspNetCore.Mvc.Extensions.Domain
{
    public interface IEntitySoftDelete
    {
       bool IsDeleted { get; set; }
       DateTime? DeletedOn { get; set; }
       string DeletedBy { get; set; }
    }
}
