using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Sqlite.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data.Migrations
{
    public class CompositeMigrationsAnnotationsProvider : IMigrationsAnnotationProvider
    {
        private readonly IMigrationsAnnotationProvider[] _providers;

        public CompositeMigrationsAnnotationsProvider(MigrationsAnnotationProviderDependencies dependencies)
        {
            _providers = new IMigrationsAnnotationProvider[] {
                new SqlServerMigrationsAnnotationProvider(dependencies),
                new SqliteMigrationsAnnotationProvider(dependencies)
            };
        }

        public IEnumerable<IAnnotation> For(IModel model) => _providers.SelectMany(p => p.For(model));
        public IEnumerable<IAnnotation> For(IProperty property) => _providers.SelectMany(p => p.For(property));
        public IEnumerable<IAnnotation> For(IIndex index) => _providers.SelectMany(p => p.For(index));
        public IEnumerable<IAnnotation> For(IKey key) => _providers.SelectMany(p => p.For(key));
        public IEnumerable<IAnnotation> For(IForeignKey foreignKey) => _providers.SelectMany(p => p.For(foreignKey));
        public IEnumerable<IAnnotation> For(IEntityType entityType) => _providers.SelectMany(p => p.For(entityType));
        public IEnumerable<IAnnotation> For(ISequence sequence) => _providers.SelectMany(p => p.For(sequence));

        public IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint) => _providers.SelectMany(p => p.For(checkConstraint));

        public IEnumerable<IAnnotation> ForRemove(IModel model) => _providers.SelectMany(p => p.ForRemove(model));
        public IEnumerable<IAnnotation> ForRemove(IIndex index) => _providers.SelectMany(p => p.ForRemove(index));
        public IEnumerable<IAnnotation> ForRemove(IProperty property) => _providers.SelectMany(p => p.ForRemove(property));
        public IEnumerable<IAnnotation> ForRemove(IKey key) => _providers.SelectMany(p => p.ForRemove(key));
        public IEnumerable<IAnnotation> ForRemove(IForeignKey foreignKey) => _providers.SelectMany(p => p.ForRemove(foreignKey));
        public IEnumerable<IAnnotation> ForRemove(IEntityType entityType) => _providers.SelectMany(p => p.ForRemove(entityType));
        public IEnumerable<IAnnotation> ForRemove(ISequence sequence) => _providers.SelectMany(p => p.ForRemove(sequence));

        public IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint) => _providers.SelectMany(p => p.ForRemove(checkConstraint));
    }
}
