using System;

namespace AspNetCore.Mvc.Extensions.Domain
{
    public interface IEntity
    {
        object Id { get; set; }
    }

    public interface IEntity<T> : IEntity where T : IEquatable<T>
    {
        new T Id { get; set; }
    }
}
