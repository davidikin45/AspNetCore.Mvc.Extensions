using AspNetCore.Mvc.Extensions.Data.Repository;
using AspNetCore.Mvc.Extensions.Domain;
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
    //services.AddScoped<IUnitOfWork, AppUnitOfWork>();
    //Only save changes if we're not a nested scope. Otherwise, let the top-level scope decide when the changes should be saved.
    public abstract class UnitOfWork : IUnitOfWork
    {
        public bool CommitingChanges { get; set; }

        protected readonly bool validateOnSave;
        protected readonly IValidationService validationService;

        protected List<DbContext> contexts = new List<DbContext>();

        protected readonly Dictionary<Type, DbContext> contextsByEntityType = new Dictionary<Type, DbContext>();
        protected readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public bool AutoDetectChangesEnabled
        {
            get
            {
                return contexts.All(c => c.ChangeTracker.AutoDetectChangesEnabled);
            }
            set
            {
                contexts.ForEach(c => c.ChangeTracker.AutoDetectChangesEnabled = value);
            }
        }

        public QueryTrackingBehavior QueryTrackingBehavior
        {
            get
            {
                return contexts.All(c => c.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.NoTracking) ? QueryTrackingBehavior.NoTracking : QueryTrackingBehavior.TrackAll;
            }
            set
            {
                contexts.ForEach(c => c.ChangeTracker.QueryTrackingBehavior = value);
            }
        }

        public UnitOfWork(bool validateOnSave, IValidationService validationService, params DbContext[] contexts)
            : this(contexts)
        {
            this.validateOnSave = validateOnSave;
            this.validationService = validationService;
        }

        public UnitOfWork(params DbContext[] contexts)
        {
            foreach (var context in contexts)
            {
                this.contexts.Add(context);
                foreach (var modelType in context.GetModelTypes())
                {
                    contextsByEntityType.Add(modelType, context);
                }
            }
            InitializeRepositories(contextsByEntityType);
        }

        public abstract void InitializeRepositories(Dictionary<Type, DbContext> contextsByEntityType);

        public void AddRepository<TEntity>(IGenericRepository<TEntity> repository) where TEntity : class
        {
            var key = typeof(TEntity);
            AddRepository(key, repository);
        }

        public void AddRepository(Type key, object repository)
        {
            repositories.Add(key, repository);
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var key = typeof(TEntity);
            if (!repositories.ContainsKey(key))
            {
                AddRepository<TEntity>(new GenericRepository<TEntity>(contextsByEntityType[key]));
            }

            return (IGenericRepository<TEntity>)repositories[key];
        }

        public DbContext DbContextByEntityType<TEntity>()
        {
            return DbContextByEntityType(typeof(TEntity));
        }

        public DbContext DbContextByEntityType(Type entityType)
        {
            return contextsByEntityType[entityType];
        }

        #region Validation
        public virtual Result GetValidationErrors()
        {
            foreach (var context in contexts)
            {
                var errors = GetValidationErrors(context);
                if (errors.Count() > 0)
                {
                    return Result.DatabaseErrors(errors);
                }
            }

            return Result.Ok();
        }

        private IEnumerable<DbEntityValidationResult> GetValidationErrors(DbContext context)
        {
            var list = new List<DbEntityValidationResult>();

            var entities = context.ChangeTracker.Entries().Where(e => ((e.State == EntityState.Added) || (e.State == EntityState.Modified)));

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
        public virtual Result<int> Complete()
        {
            return CompleteAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task<Result<int>> CompleteAsync()
        {
            return CompleteAsync(CancellationToken.None);
        }

        public virtual async Task<Result<int>> CompleteAsync(CancellationToken cancellationToken)
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

                var changes = 0;

                if (commitChanges && lastError == null)
                {
                    foreach (var context in contexts)
                    {
                        try
                        {
                            changes += await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

        #region Concurrency
        protected Result<int> HandleEFCoreUpdateAndDeleteConcurrency(DbUpdateConcurrencyException ex)
        {
            var errors = new List<ValidationResult>();

            var entry = ex.Entries.Single();
            var clientValues = entry.Entity;
            var databaseEntry = entry.GetDatabaseValues();
            if (databaseEntry == null)
            {
                return Result.ConcurrencyConflict<int>("Unable to save changes. Object was deleted by another user.");
            }

            var databaseValues = databaseEntry.ToObject();

            foreach (var prop in databaseValues.GetProperties())
            {
                var v1 = clientValues.GetPropValue(prop.Name);
                var v2 = databaseValues.GetPropValue(prop.Name);

                if (!(v1 == null && v2 == null))
                {
                    if (((v1 == null && v2 != null) || (v2 == null && v1 != null) || !v1.Equals(v2)))
                    {
                        var v2String = v2 == null ? "" : v2.ToString();

                        errors.Add(new ValidationResult("Current value: " + v2String, new string[] { prop.Name }));
                    }
                }
            }

            errors.Add(new ValidationResult("The record you attempted to edit or delete "
                + "was modified by another user after you got the original value. The "
                + "operation was canceled and the current values in the database "
                + "have been returned. If you still want to perform this opeartion on the record, save "
                + "again."));

            return Result.ConcurrencyConflict<int>(errors, ((IEntityConcurrencyAware)databaseValues).RowVersion);
        }
        #endregion
    }
}
