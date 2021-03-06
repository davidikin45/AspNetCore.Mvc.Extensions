﻿using AspNetCore.Mvc.Extensions.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class DbContextSaveExtensions
    {
        public static TDbContext SetTimestamps<TDbContext>(this TDbContext context) where TDbContext : DbContext
        {
            var added = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e => e.Entity).Where(x => x is IEntityAuditable);
            var modified = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e => e.Entity).Where(x => x is IEntityAuditable);
            var deleted = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted && e.Entity is IEntitySoftDelete);

            foreach (var entity in added)
            {

                ((IEntityAuditable)entity).CreatedOn = DateTime.UtcNow;

                ((IEntityAuditable)entity).UpdatedOn = DateTime.UtcNow;
            }

            foreach (var entity in modified)
            {

                ((IEntityAuditable)entity).UpdatedOn = DateTime.UtcNow;
            }

            foreach (var entityEntry in deleted)
            {
                entityEntry.State = EntityState.Modified;
                ((IEntitySoftDelete)entityEntry.Entity).IsDeleted = true;
                ((IEntitySoftDelete)entityEntry.Entity).DeletedOn = DateTime.UtcNow;
            }

            return context;
        }

        //https://blog.victorleonardo.com/en/audit-trail-with-entity-framework-core/
        //https://www.meziantou.net/entity-framework-core-history-audit-table.htm
        private static Type EntityAuditableType = typeof(IEntityAuditable);

        public static List<AuditEntry> AuditBeforeSaveChanges(this DbContext context)
        {
            var auditEntries = new List<AuditEntry>();

            if (context.GetModelTypes().Contains(typeof(Audit)))
            {
                context.ChangeTracker.DetectChanges();
                foreach (var entry in context.ChangeTracker.Entries())
                {
                    if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                        continue;

                    var auditEntry = new AuditEntry(entry);

                     auditEntry.TableName = entry.Metadata.GetTableName();
                    //.NET Core 2.2 
                    //auditEntry.TableName = entry.Metadata.Relational().TableName;

                    auditEntries.Add(auditEntry);

                    foreach (var property in entry.Properties)
                    {
                        if (EntityAuditableType.GetProperty(property.Metadata.Name) != null)
                            continue;

                        if (property.IsTemporary)
                        {
                            // value will be generated by the database, get the value after saving
                            auditEntry.TemporaryProperties.Add(property);
                            continue;
                        }

                        string propertyName = property.Metadata.Name;
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[propertyName] = property.CurrentValue;
                            continue;
                        }

                        switch (entry.State)
                        {
                            case EntityState.Added:
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                break;

                            case EntityState.Deleted:
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                break;

                            case EntityState.Modified:
                                if (property.IsModified)
                                {
                                    if (property.OriginalValue == null && property.CurrentValue == null)
                                        continue;

                                    if (property.OriginalValue == null ||
                                       property.CurrentValue == null ||
                                       !property.OriginalValue.Equals(property.CurrentValue))
                                    {
                                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                                    }
                                }
                                break;
                        }
                    }
                }

                // Save audit entities that have all the modifications
                foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
                {
                    context.Add(auditEntry.ToAudit());
                }
            }

            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        public static void AuditAfterSaveChanges(this DbContext context, List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            foreach (var auditEntry in auditEntries)
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Save the Audit entry
                context.Add(auditEntry.ToAudit());
            }
        }

        public static Task AuditAfterSaveChangesAsync(this DbContext context, List<AuditEntry> auditEntries, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Save the Audit entry
                context.Add(auditEntry.ToAudit());
            }

            return context.SaveChangesAsync(cancellationToken);
        }
    }
}
