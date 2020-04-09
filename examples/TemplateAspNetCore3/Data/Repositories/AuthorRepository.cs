using AspNetCore.Mvc.Extensions.Data.Repository;
using AspNetCore.Mvc.Extensions.Helpers;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Data.Repositories
{
    public interface IAuthorRepository : IGenericRepository<Author>
    {
        Task<Author> GetAuthorAsync(string authorSlug, CancellationToken cancellationToken);
    }

    public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(AppContext context)
            : base(context)
        {

        }

        public Task<Author> GetAuthorAsync(string authorSlug, CancellationToken cancellationToken)
        {
            return GetFirstAsync(cancellationToken, c => c.UrlSlug.Equals(authorSlug));
        }

        public override Author Add(Author entity, string addedBy)
        {
            if (string.IsNullOrEmpty(entity.UrlSlug))
            {
                entity.UrlSlug = UrlSlugger.ToUrlSlug(entity.Name);
            }

            return base.Add(entity, addedBy);
        }

        public override Author Update(Author entity, string updatedBy)
        {
            if (string.IsNullOrEmpty(entity.UrlSlug))
            {
                entity.UrlSlug = UrlSlugger.ToUrlSlug(entity.Name);
            }

            return base.Update(entity, updatedBy);
        }
    }
}
