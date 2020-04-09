using AspNetCore.Mvc.Extensions.DomainEvents;
using System;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Domain
{
    public abstract class EntityBase<T> : IEntity<T>, IEntityAuditable where T : IEquatable<T>
    {
        int? _requestedHashCode;

        public virtual T Id { get; set; }

        object IEntity.Id
        {
            get { return this.Id; }
            set { this.Id = (T)value; }
        }

        //EF requires an empty constructor
        protected EntityBase()
        {
        }

        public virtual DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as EntityBase<T>;

            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
                return false;

            if (this.IsTransient() || other.IsTransient())
                return false;

            return Id.Equals(other.Id);
        }

        public static bool operator ==(EntityBase<T> a, EntityBase<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(EntityBase<T> a, EntityBase<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = (GetType().ToString() + this.Id.ToString()).GetHashCode() ^ 31;
                // XOR for random distribution. See:
                // https://blogs.msdn.microsoft.com/ericlippert/2011/02/28/guidelines-and-rules-for-gethashcode/
                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();
        }

        public bool IsTransient()
        {
            return Id.Equals(default);
        }
    }
}
