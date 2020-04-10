using AspNetCore.Mvc.Extensions.DomainEvents;
using System;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Domain
{
    public abstract class EntityAggregateRootBase<T> : EntityBase<T>, IEntityAggregateRoot, IEntityDomainEvents, IEntityConcurrencyAware where T : IEquatable<T>
    {
        //Optimistic Concurrency. Potentially ETags serve the same purpose
        public byte[] RowVersion { get; set; }

       
        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();
    
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(DomainEvent eventItem)
        {
            _domainEvents.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
