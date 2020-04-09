using AspNetCore.Mvc.Extensions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class DbContextModelBuilderExtensions
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
             entity.SetTableName(entity.DisplayName());
             //.NET Core 2.2 
             //entity.Relational().TableName = entity.DisplayName();
            }
        }

        public static void AddSoftDeleteFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDeletedProperty = entityType.FindProperty(nameof(IEntitySoftDelete.IsDeleted));
                if (isDeletedProperty != null && isDeletedProperty.ClrType == typeof(bool))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "p");
                    var filter = Expression.Lambda(Expression.Not(Expression.Property(parameter, isDeletedProperty.PropertyInfo)), parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }

        public static void AddBackingFields(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties().Where(p => p.PropertyInfo != null))
                {
                    var attributes = property.PropertyInfo.GetCustomAttributes(typeof(HasFieldAttribute), false);
                    if (attributes != null && attributes.Any())
                    {
                        property.SetPropertyAccessMode(PropertyAccessMode.Field);
                    }
                }
            }
        }

        public static void AddDateTimeUtc(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        modelBuilder.Entity(entityType.ClrType)
                         .Property<DateTime>(property.Name)
                         .HasConversion(
                          v => v.ToUniversalTime(),
                          v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        modelBuilder.Entity(entityType.ClrType)
                         .Property<DateTime?>(property.Name)
                         .HasConversion(
                          v => v.HasValue ? v.Value.ToUniversalTime() : v,
                          v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                    }
                }
            }
        }
    }


    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class HasFieldAttribute : Attribute
    { }
}