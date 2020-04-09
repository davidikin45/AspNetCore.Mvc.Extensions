using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore.Mvc.Extensions.Domain
{
    //Should always be owned!
    public abstract class EntityChildBase<T> : EntityBase<T>, IEntityChild where T : IEquatable<T>
    {
        [NotMapped]
        public TrackingState TrackingState { get; set; }
    }
}
