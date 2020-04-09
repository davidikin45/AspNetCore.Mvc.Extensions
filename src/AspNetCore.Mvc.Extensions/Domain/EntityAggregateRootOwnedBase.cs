using System;

namespace AspNetCore.Mvc.Extensions.Domain
{
    public abstract class EntityAggregateRootOwnedBase<T> : EntityAggregateRootBase<T>, IEntityOwned where T : IEquatable<T>
    {
        public string OwnedBy { get; set; }
    }
}
