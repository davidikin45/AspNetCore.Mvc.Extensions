using AspNetCore.Mvc.Extensions.Data.DomainEvents;
using AspNetCore.Mvc.Extensions.DomainEvents;
using AspNetCore.Mvc.Extensions.Validation;
using AspNetCore.Mvc.Extensions.Validation.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.UnitOfWork
{
    //https://github.com/aspnet/EntityFrameworkCore/issues/9237
    //services.AddScoped<IUnitOfWork, AppUnitOfWork>();
    public abstract class UnitOfWorkWithEvents : UnitOfWork
    {
        protected List<(DbContext dbContext, DbContextDomainEventsEFCoreAdapter domainEvents)> contextsWithDomainEvents = new List<(DbContext dbContext, DbContextDomainEventsEFCoreAdapter domainEvents)>();

        public UnitOfWorkWithEvents(bool validateOnSave, IValidationService validationService, IDomainEventBus domainEventBus, params DbContext[] contexts)
            : base(validateOnSave, validationService, contexts)
        {
            foreach (var context in contexts)
            {
                this.contextsWithDomainEvents.Add((context, new DbContextDomainEventsEFCoreAdapter(context, domainEventBus)));
            }
        }

        #region Validation
        public override Result GetValidationErrors()
        {
            foreach (var context in contextsWithDomainEvents)
            {
                var errors = GetValidationErrors(context.dbContext, context.domainEvents, true);
                if (errors.Count() > 0)
                {
                    return Result.DatabaseErrors(errors);
                }
            }

            return Result.Ok();
        }

        private IEnumerable<DbEntityValidationResult> GetValidationErrors(DbContext context, DbContextDomainEventsEFCoreAdapter domainEvents, bool newChanges)
        {
            var list = new List<DbEntityValidationResult>();

            var entities = context.ChangeTracker.Entries().Where(e => ((e.State == EntityState.Added) || (e.State == EntityState.Modified)));
            if (newChanges)
            {
                entities = entities.Where(x => !domainEvents.GetPreCommittedDeletedEntities().Contains(x) && !domainEvents.GetPreCommittedInsertedEntities().Contains(x));
            }

            foreach (var entry in entities)
            {
                var entity = entry.Entity;

                var results = validationService.ValidateObject(entity);

                if (results.Count() > 0)
                {
                    var errors = results.Where(r => r != ValidationResult.Success);

                    if (errors.Count() > 0)
                    {
                        var dbValidationErrors = new List<DbValidationError>();
                        foreach (ValidationResult error in errors)
                        {
                            if (error.MemberNames.Count() > 0)
                            {
                                foreach (var prop in error.MemberNames)
                                {
                                    dbValidationErrors.Add(new DbValidationError(prop, error.ErrorMessage));
                                }
                            }
                            else
                            {
                                dbValidationErrors.Add(new DbValidationError("", error.ErrorMessage));
                            }
                        }

                        var validationResult = new DbEntityValidationResult(dbValidationErrors);

                        list.Add(validationResult);
                    }
                }
            }

            return list;
        }
        #endregion

        #region Save Changes
        public override Result<int> Complete()
        {
            return CompleteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public override Task<Result<int>> CompleteAsync()
        {
            return CompleteAsync(CancellationToken.None);
        }

        public override async Task<Result<int>> CompleteAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (validateOnSave)
                {
                    var validationResult = GetValidationErrors();
                    if (!validationResult.IsSuccess)
                    {
                        return Result.DatabaseErrors<int>(validationResult.ObjectValidationErrors);
                    }
                }

                bool commitChanges = !CommitingChanges;
                CommitingChanges = true;

                ExceptionDispatchInfo lastError = null;
                foreach (var context in contextsWithDomainEvents)
                {
                    try
                    {
                        await context.domainEvents.FirePreCommitEventsAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        lastError = ExceptionDispatchInfo.Capture(e);
                    }
                }

                var changes = 0;

                if (commitChanges && lastError == null)
                {
                    foreach (var context in contextsWithDomainEvents)
                    {
                        try
                        {
                            await context.domainEvents.FirePreCommitEventsAsync().ConfigureAwait(false);
                            changes += await context.dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            await context.domainEvents.FirePostCommitEventsAsync().ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            lastError = ExceptionDispatchInfo.Capture(e);
                        }
                    }
                }

                if (commitChanges)
                {
                    CommitingChanges = false;
                }

                if (lastError != null)
                    lastError.Throw(); // Re-throw while maintaining the exception's original stack track

                return Result.Ok(changes);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return HandleEFCoreUpdateAndDeleteConcurrency(ex);
            }
        }
        #endregion
    }
}
