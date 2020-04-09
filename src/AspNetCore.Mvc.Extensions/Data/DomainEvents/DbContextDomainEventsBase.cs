using AspNetCore.Mvc.Extensions.DomainEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Mvc.Extensions.Domain;

namespace AspNetCore.Mvc.Extensions.Data.DomainEvents
{
    public abstract class DbContextDomainEventsBase : IDbContextDomainEvents
    {
        private IDomainEventBus _domainEventBus;
        public DbContextDomainEventsBase(IDomainEventBus domainEventBus)
        {
            _domainEventBus = domainEventBus;
        }

        protected Dictionary<object, List<DomainEvent>> precommitedUpdatedEvents = new Dictionary<object, List<DomainEvent>>();
        protected List<object> precommitedUpdatedEntities = new List<object>();
        protected Dictionary<object, List<DomainEvent>> precommitedPropertyUpdateEvents = new Dictionary<object, List<DomainEvent>>();
        protected Dictionary<object, List<DomainEvent>> precommitedDeletedEvents = new Dictionary<object, List<DomainEvent>>();
        protected List<object> precommitedDeletedEntities = new List<object>();
        protected Dictionary<object, List<DomainEvent>> precommitedInsertedEvents = new Dictionary<object, List<DomainEvent>>();
        protected List<object> precommitedInsertedEntities = new List<object>();
        protected Dictionary<object, List<DomainEvent>> precommitedDomainEvents = new Dictionary<object, List<DomainEvent>>();

        public async Task FirePreCommitEventsAsync()
        {
            var updatedEvents = GetNewUpdatedEvents();
            precommitedUpdatedEntities.AddRange(updatedEvents.Keys);
            precommitedUpdatedEvents = precommitedUpdatedEvents.Concat(updatedEvents).ToDictionary(x => x.Key, x => x.Value);

            var propertiesUpdatedEvents = GetNewPropertyUpdatedEvents();
            foreach (var entity in propertiesUpdatedEvents)
            {
                if (!precommitedPropertyUpdateEvents.ContainsKey(entity.Key))
                {
                    precommitedPropertyUpdateEvents.Add(entity.Key, new List<DomainEvent>());
                }

                foreach (var ev in entity.Value)
                {
                    precommitedPropertyUpdateEvents[entity.Key].Add(ev);
                }
            }

            var deletedEvents = GetNewDeletedEvents();
            precommitedDeletedEntities.AddRange(deletedEvents.Keys);
            precommitedDeletedEvents = precommitedDeletedEvents.Concat(deletedEvents).ToDictionary(x => x.Key, x => x.Value);

            var insertedEvents = GetNewInsertedEvents();
            precommitedInsertedEntities.AddRange(insertedEvents.Keys);
            precommitedInsertedEvents = precommitedInsertedEvents.Concat(insertedEvents).ToDictionary(x => x.Key, x => x.Value);

            var domainEvents = GetNewDomainEvents();
            foreach (var entity in domainEvents)
            {
                if (!precommitedDomainEvents.ContainsKey(entity.Key))
                {
                    precommitedDomainEvents.Add(entity.Key, new List<DomainEvent>());
                }

                foreach (var ev in entity.Value)
                {
                    precommitedDomainEvents[entity.Key].Add(ev);
                }
            }

            if (_domainEventBus != null)
            {
                await DispatchDomainEventsPreCommitAsync(updatedEvents, propertiesUpdatedEvents, deletedEvents, insertedEvents, domainEvents).ConfigureAwait(false);
            }
        }

        public async Task FirePostCommitEventsAsync()
        {
            try
            {
                if (_domainEventBus != null)
                {
                    await DispatchDomainEventsPostCommitAsync(precommitedUpdatedEvents, precommitedPropertyUpdateEvents, precommitedDeletedEvents, precommitedInsertedEvents, precommitedDomainEvents).ConfigureAwait(false);
                }
            }
            finally
            {
                precommitedUpdatedEvents.Clear();
                precommitedUpdatedEntities.Clear();
                precommitedPropertyUpdateEvents.Clear();
                precommitedDeletedEvents.Clear();
                precommitedDeletedEntities.Clear();
                precommitedInsertedEvents.Clear();
                precommitedInsertedEntities.Clear();
                precommitedDomainEvents.Clear();
            }
        }

        public Dictionary<object, List<DomainEvent>> GetNewDeletedEvents()
        {
            var entities = GetNewDeletedEntities();
            var events = CreateEntityDeletedEvents(entities);

            if (events == null)
            {
                events = new Dictionary<object, List<DomainEvent>>();
            }

            return events;
        }

        public IEnumerable<object> GetNewDeletedEntities()
        {
            var entities = GetDeletedEntities().Where(x => !precommitedDeletedEntities.Contains(x)).ToList();
            return entities;
        }

        public IEnumerable<object> GetPreCommittedDeletedEntities()
        {
            return precommitedDeletedEntities;
        }

        public Dictionary<object, List<DomainEvent>> GetNewInsertedEvents()
        {
            var entities = GetNewInsertedEntities();
            var events = CreateEntityInsertedEvents(entities);

            if (events == null)
            {
                events = new Dictionary<object, List<DomainEvent>>();
            }

            return events;
        }

        public IEnumerable<object> GetNewInsertedEntities()
        {
            var entities = GetInsertedEntities().Where(x => !precommitedInsertedEntities.Contains(x)).ToList();
            return entities;
        }

        public IEnumerable<object> GetPreCommittedInsertedEntities()
        {
            return precommitedInsertedEntities;
        }

        public Dictionary<object, List<DomainEvent>> GetNewUpdatedEvents()
        {
            var entities = GetNewUpdatedEntities();
            var events = CreateEntityUpdatedEvents(entities);

            if (events == null)
            {
                events = new Dictionary<object, List<DomainEvent>>();
            }

            return events;
        }

        public IEnumerable<object> GetNewUpdatedEntities()
        {
            var entities = GetUpdatedEntities().Where(x => !precommitedUpdatedEntities.Contains(x)).ToList();
            return entities;
        }

        public IEnumerable<object> GetPreCommittedUpdatedEntities()
        {
            return precommitedUpdatedEntities;
        }

        public Dictionary<object, List<DomainEvent>> GetNewDomainEvents()
        {
            var entities = GetUpdatedDeletedInsertedEntities().ToList();

            var events = CreateEntityDomainEvents(entities);

            if (events == null)
            {
                events = new Dictionary<object, List<DomainEvent>>();
            }

            return events;
        }

        protected abstract Dictionary<object, List<DomainEvent>> GetNewPropertyUpdatedEvents();
        protected abstract IEnumerable<object> GetUpdatedDeletedInsertedEntities();
        protected abstract IEnumerable<object> GetUpdatedEntities();
        protected abstract IEnumerable<object> GetInsertedEntities();
        protected abstract IEnumerable<object> GetDeletedEntities();

        public Dictionary<object, List<DomainEvent>> CreateEntityUpdatedEvents(IEnumerable<object> updatedObjects)
        {
            var dict = new Dictionary<object, List<DomainEvent>>();
            var updated = updatedObjects.Where(x => x is IEntity).Cast<IEntity>();

            foreach (var entity in updated)
            {
                var events = new List<DomainEvent>();
                Type genericType = typeof(EntityUpdatedEvent<>);
                Type[] typeArgs = { entity.GetType() };
                Type constructed = genericType.MakeGenericType(typeArgs);
                string updatedBy = null;
                if (entity is IEntityAuditable)
                {
                    updatedBy = ((IEntityAuditable)entity).UpdatedBy;
                }
                DomainEvent domainEvent = (DomainEvent)Activator.CreateInstance(constructed, entity, updatedBy);
                events.Add(domainEvent);
                dict.Add(entity, events);
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

        public Dictionary<object, List<DomainEvent>> CreateEntityDeletedEvents(IEnumerable<object> deletedObjects)
        {
            var dict = new Dictionary<object, List<DomainEvent>>();
            var deleted = deletedObjects.Where(x => x is IEntity).Cast<IEntity>();

            foreach (var entity in deleted)
            {
                var events = new List<DomainEvent>();
                Type genericType = typeof(EntityDeletedEvent<>);
                Type[] typeArgs = { entity.GetType() };
                Type constructed = genericType.MakeGenericType(typeArgs);
                string deletedBy = null;
                DomainEvent domainEvent = (DomainEvent)Activator.CreateInstance(constructed, entity, deletedBy);
                events.Add(domainEvent);
                dict.Add(entity, events);
            }

            return dict;
        }

        public Dictionary<object, List<DomainEvent>> CreateEntityInsertedEvents(IEnumerable<object> insertedObjects)
        {
            var dict = new Dictionary<object, List<DomainEvent>>();
            var inserted = insertedObjects.Where(x => x is IEntity).Cast<IEntity>();

            foreach (var entity in inserted)
            {
                var events = new List<DomainEvent>();
                Type genericType = typeof(EntityInsertedEvent<>);
                Type[] typeArgs = { entity.GetType() };
                Type constructed = genericType.MakeGenericType(typeArgs);
                string createdBy = null;
                if (entity is IEntityAuditable)
                {
                    createdBy = ((IEntityAuditable)entity).CreatedBy;
                }
                var domainEvent = (DomainEvent)Activator.CreateInstance(constructed, entity, createdBy);
                events.Add(domainEvent);
                dict.Add(entity, events);
            }

            return dict;
        }

        public Dictionary<object, List<DomainEvent>> CreateEntityDomainEvents(IEnumerable<object> updatedDeletedInsertedObjects)
        {
            var dict = new Dictionary<object, List<DomainEvent>>();
            var updatedDeletedInserted = updatedDeletedInsertedObjects.Where(x => x is IEntity).Cast<IEntity>();

            foreach (var entity in updatedDeletedInserted)
            {
                var events = new List<DomainEvent>();
                if (entity is IEntityDomainEvents)
                {
                    var domainEventsEntity = ((IEntityDomainEvents)entity);
                    var entityEvents = domainEventsEntity.DomainEvents.ToArray();
                    foreach (var domainEvent in entityEvents)
                    {
                        events.Add(domainEvent);
                    }
                    domainEventsEntity.ClearDomainEvents();
                }
                dict.Add(entity, events);
            }

            return dict;
        }

        //If you are handling the domain events right before committing the original transaction is because you want the side effects of those events to be included in the same transaction
        private async Task DispatchDomainEventsPreCommitAsync(
       Dictionary<object, List<DomainEvent>> entityUpdatedEvents,
       Dictionary<object, List<DomainEvent>> propertyUpdatedEvents,
       Dictionary<object, List<DomainEvent>> entityDeletedEvents,
       Dictionary<object, List<DomainEvent>> entityInsertedEvents,
       Dictionary<object, List<DomainEvent>> entityDomainEvents
       )
        {
            foreach (var kvp in entityUpdatedEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    await _domainEventBus.PublishPreCommitAsync(domainEvent).ConfigureAwait(false);
                }

                //Property Update Events
                if (propertyUpdatedEvents != null && propertyUpdatedEvents.ContainsKey(kvp.Key))
                {
                    foreach (var propertyUpdateEvent in propertyUpdatedEvents[kvp.Key])
                    {
                        await _domainEventBus.PublishPreCommitAsync(propertyUpdateEvent).ConfigureAwait(false);
                    }
                }
            }

            foreach (var kvp in entityDeletedEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    await _domainEventBus.PublishPreCommitAsync(domainEvent).ConfigureAwait(false);
                }
            }

            foreach (var kvp in entityInsertedEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    await _domainEventBus.PublishPreCommitAsync(domainEvent).ConfigureAwait(false);
                }
            }

            foreach (var kvp in entityDomainEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    await _domainEventBus.PublishPreCommitAsync(domainEvent).ConfigureAwait(false);
                }
            }
        }

        //If you are handling the domain events after committing the original transaction is because you do not want the side effects of those events to be included in the same transaction. e.g sending an email
        private async Task DispatchDomainEventsPostCommitAsync(
       Dictionary<object, List<DomainEvent>> entityUpdatedEvents,
       Dictionary<object, List<DomainEvent>> propertyUpdatedEvents,
       Dictionary<object, List<DomainEvent>> entityDeletedEvents,
       Dictionary<object, List<DomainEvent>> entityInsertedEvents,
       Dictionary<object, List<DomainEvent>> entityDomainEvents)
        {


            var domainEvents = new List<DomainEvent>();

            foreach (var kvp in entityUpdatedEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    try
                    {
                        domainEvents.Add(domainEvent);
                        //await _domainEvents.DispatchPostCommitAsync(domainEvent).ConfigureAwait(false);
                    }
                    catch
                    {

                    }
                }

                //Property Update Events
                if (propertyUpdatedEvents != null && propertyUpdatedEvents.ContainsKey(kvp.Key))
                {
                    foreach (var propertyUpdateEvent in propertyUpdatedEvents[kvp.Key])
                    {
                        try
                        {
                            domainEvents.Add(propertyUpdateEvent);
                            //await _domainEvents.DispatchPostCommitAsync(propertyUpdateEvent).ConfigureAwait(false);
                        }
                        catch
                        {

                        }
                    }
                }
            }

            foreach (var kvp in entityDeletedEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    try
                    {
                        domainEvents.Add(domainEvent);
                        //await _domainEvents.DispatchPostCommitAsync(domainEvent).ConfigureAwait(false);
                    }
                    catch
                    {

                    }

                }
            }

            foreach (var kvp in entityInsertedEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    try
                    {
                        domainEvents.Add(domainEvent);
                        //await _domainEvents.DispatchPostCommitAsync(domainEvent).ConfigureAwait(false);
                    }
                    catch
                    {

                    }

                }
            }

            foreach (var kvp in entityDomainEvents)
            {
                foreach (var domainEvent in kvp.Value)
                {
                    try
                    {
                        domainEvents.Add(domainEvent);
                        //await _domainEvents.DispatchPostCommitAsync(domainEvent).ConfigureAwait(false);
                    }
                    catch
                    {

                    }

                }
            }

            await _domainEventBus.PublishPostCommitBatchAsync(domainEvents).ConfigureAwait(false);
        }
    }
}
