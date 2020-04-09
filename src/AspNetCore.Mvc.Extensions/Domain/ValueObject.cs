using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Domain
{
    //Always valid
    //since they are immutable, you can validate them on creation and never worry about validation again. If the state cannot be changed, then you know it's always valid.
    //Interchangeable. e.g 50 cents
    //No identity and Immutable
    //value objects as static data that will never change and entities as data that evolves in your application.
    //https://docs.microsoft.com/en-us/ef/core/modeling/owned-entities
    //https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
    [Owned]
    public abstract class ValueObject : IValidatableObject
    {
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !(EqualOperator(left, right));
        }

        protected abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            ValueObject other = (ValueObject)obj;
            IEnumerator<object> thisValues = GetAtomicValues().GetEnumerator();
            IEnumerator<object> otherValues = other.GetAtomicValues().GetEnumerator();
            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (ReferenceEquals(thisValues.Current, null) ^
                    ReferenceEquals(otherValues.Current, null))
                {
                    return false;
                }

                if (thisValues.Current != null &&
                    !thisValues.Current.Equals(otherValues.Current))
                {
                    return false;
                }
            }
            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            return GetAtomicValues()
             .Select(x => x != null ? x.GetHashCode() : 0)
             .Aggregate((x, y) => x ^ y);
        }

        //1. [Required]
        //2. Other attributes
        //3. IValidatableObject Implementation
        public IEnumerable<ValidationResult> Validate()
        {
            var context = new ValidationContext(this);

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                context.ObjectInstance,
                context,
               validationResults,
               validateAllProperties: true); // if true [Required] + Other attributes

            return validationResults.Where(r => r != ValidationResult.Success);
        }

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }

    //https://gist.github.com/marisks/f9938777ba590b1645376e783f5980ff
    //http://grabbagoft.blogspot.com/2007/06/generic-value-object-equality.html
    public abstract class ValueObject<T> : IEquatable<T>, IValidatableObject
    where T : ValueObject<T>
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var other = obj as T;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            var fields = GetFields(this);

            var startValue = 17;
            var multiplier = 59;

            return fields
                .Select(field => field.GetValue(this))
                .Where(value => value != null)
                .Aggregate(
                    startValue,
                        (current, value) => current * multiplier + value.GetHashCode());
        }

        public virtual bool Equals(T other)
        {
            if (other == null)
                return false;

            var t = GetType();
            var otherType = other.GetType();

            if (t != otherType)
                return false;

            var fields = GetFields(this);

            foreach (var field in fields)
            {
                var value1 = field.GetValue(other);
                var value2 = field.GetValue(this);

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

        private static IEnumerable<FieldInfo> GetFields(object obj)
        {
            var t = obj.GetType();

            var fields = new List<FieldInfo>();

            while (t != typeof(object))
            {
                if (t == null) continue;
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                t = t.BaseType;
            }

            return fields;
        }

        public static bool operator ==(ValueObject<T> x, ValueObject<T> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(ValueObject<T> x, ValueObject<T> y)
        {
            return !(x == y);
        }

        public bool IsEmpty()
        {
            Type t = GetType();
            FieldInfo[] fields = t.GetFields
              (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(this);
                if (value != null)
                {
                    return false;
                }
            }
            return true;
        }

        public ValueObject<T> GetCopy()
        {
            return MemberwiseClone() as ValueObject<T>;
        }

        //1. [Required]
        //2. Other attributes
        //3. IValidatableObject Implementation
        public IEnumerable<ValidationResult> Validate()
        {
            var context = new ValidationContext(this);

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                context.ObjectInstance,
                context,
               validationResults,
               validateAllProperties: true); // if true [Required] + Other attributes

            return validationResults.Where(r => r != ValidationResult.Success);
        }

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }
}
