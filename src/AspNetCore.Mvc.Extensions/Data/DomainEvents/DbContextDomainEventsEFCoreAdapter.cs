using AspNetCore.Mvc.Extensions.Domain;
using AspNetCore.Mvc.Extensions.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data.DomainEvents
{
    public class DbContextDomainEventsEFCoreAdapter : DbContextDomainEventsBase
    {
        private DbContext _dbContext;
        public DbContextDomainEventsEFCoreAdapter(DbContext dbContext, IDomainEventBus domainEventBus)
            : base(domainEventBus)
        {
            _dbContext = dbContext;
        }

        protected override IEnumerable<object> GetDeletedEntities()
        {
            return _dbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity);
        }

        protected override IEnumerable<object> GetInsertedEntities()
        {
            return _dbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity);
        }

        protected override IEnumerable<object> GetUpdatedEntities()
        {
            return _dbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e => e.Entity);
        }

        protected override IEnumerable<object> GetUpdatedDeletedInsertedEntities()
        {
            return _dbContext.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted || e.State == EntityState.Added).Select(e => e.Entity);
        }

        protected override Dictionary<object, List<DomainEvent>> GetNewPropertyUpdatedEvents()
        {
            var entries = _dbContext.ChangeTracker.Entries().Where(x => (x.State == EntityState.Modified));
            var events = CreatePropertyUpdateEventsEFCore(entries);

            if (events == null)
            {
                events = new Dictionary<object, List<DomainEvent>>();
            }

            var allDomainEvents = precommitedPropertyUpdateEvents.Values.MergeLists();

            foreach (var entity in events)
            {
                entity.Value.RemoveAll(e => allDomainEvents.Contains(e));
            }

            return events;
        }

        private Dictionary<object, List<DomainEvent>> CreatePropertyUpdateEventsEFCore(IEnumerable<EntityEntry> updatedEntries)
        {
            var dict = new Dictionary<object, List<DomainEvent>>();
            foreach (var updatedEntry in updatedEntries.Where(e => e.Entity is IFirePropertyUpdatedEvents))
            {

                var properties = new Dictionary<string, OldAndNewValue>();
                var updatedProperties = new List<string>();

                //If we have the original in cache compare to these values, otherwise hit the Db to get the Db values.
                var dbValues = updatedEntry.OriginalValues ?? updatedEntry.GetDatabaseValues();
                if (dbValues != null)
                {
                    foreach (IProperty property in dbValues.Properties)
                    {
                        var original = dbValues[property.Name];
                        var current = updatedEntry.CurrentValues[property.Name];

                        properties.Add(property.Name, new OldAndNewValue(property.Name, original, current));

                        if (!Equals(original, current) && (!(original is byte[]) || ((original is byte[]) && !ByteArrayCompare((byte[])original, (byte[])current))))
                        {
                            updatedProperties.Add(property.Name);
                        }
                    }
                }

                var propertyUpdatedEvents = new List<DomainEvent>();
                foreach (string propertyName in updatedProperties)
                {
                    Type genericType = typeof(EntityPropertyUpdatedEvent<>);
                    Type[] typeArgs = { updatedEntry.Entity.GetType() };
                    Type constructed = genericType.MakeGenericType(typeArgs);

                    string updatedBy = "";
                    if (updatedEntry.Entity is IEntityAuditable)
                    {
                        updatedBy = ((IEntityAuditable)updatedEntry.Entity).UpdatedBy;
                    }

                    object domainEvent = Activator.CreateInstance(constructed, updatedEntry.Entity, updatedBy, properties, propertyName, properties[propertyName]);
                    propertyUpdatedEvents.Add((DomainEvent)domainEvent);
                }

                if (propertyUpdatedEvents.Count > 0)
                {
                    dict.Add(updatedEntry.Entity, propertyUpdatedEvents);
                }
            }

            return dict;
        }

        private bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null && a2 == null)
            {
                return true;
            }
            if (a1 == null)
            {
                return false;

            }
            if (a2 == null)
            {
                return false;

            }
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }
    }
}
