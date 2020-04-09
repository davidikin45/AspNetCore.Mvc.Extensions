using AspNetCore.Mvc.Extensions.Cqrs.HandlersDynamic;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data;

namespace TemplateAspNetCore3.Queries
{
    public class QueryHandler : AsyncDynamicRequestResponseQueryHandler
    {
        private readonly IAppUnitOfWork _appUnitOfWork;
        public QueryHandler(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
        }

        public override async Task<object> HandleAsync(string queryName, dynamic query, CancellationToken cancellationToken = default)
        {
            var result = await _appUnitOfWork.AuthorRepository.GetAllAsync(default);

            return result;
        }

    }
}
