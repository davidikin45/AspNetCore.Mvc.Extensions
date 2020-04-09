using AspNetCore.Mvc.Extensions.Attributes.Display;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public abstract class DtoBase
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return EqualsCore(obj);
        }

        public virtual bool EqualsCore(object other)
        {
            if (other == null)
                return false;

            Type t = GetType();
            Type otherType = other.GetType();

            if (t != otherType)
                return false;

            FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object value1 = field.GetValue(other);
                object value2 = field.GetValue(this);

                if (value1 == null)
                {
                    if (value2 != null)
                        return false;
                }
                else if (!value1.Equals(value2))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            IEnumerable<FieldInfo> fields = GetFields();

            int startValue = 17;
            int multiplier = 59;

            int hashCode = startValue;

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(this);

                if (value != null)
                    hashCode = hashCode * multiplier + value.GetHashCode();
            }

            return hashCode;
        }

        private IEnumerable<FieldInfo> GetFields()
        {
            Type t = GetType();

            List<FieldInfo> fields = new List<FieldInfo>();

            while (t != typeof(object))
            {
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                t = t.BaseType;
            }

            return fields;
        }

        public static bool operator ==(DtoBase a, DtoBase b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(DtoBase a, DtoBase b)
        {
            return !(a == b);
        }
    }

    public abstract class DtoBase<T> : DtoBase
    {
        [ReadOnlyHiddenInput(ShowForCreate = true, ShowForEdit = true), Display(Order = 0)]
        public virtual T Id { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as DtoBase<T>;

            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Id.ToString() == "0" || other.Id.ToString() == "0")
                return false;

            return Id.Equals(other.Id);
        }

        public static bool operator ==(DtoBase<T> a, DtoBase<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(DtoBase<T> a, DtoBase<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().ToString() + (Id != null ? Id.ToString() : "")).GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
