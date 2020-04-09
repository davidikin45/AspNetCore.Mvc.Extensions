using AspNetCore.Mvc.Extensions.Cqrs.HandlersDynamic;
using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Commands
{
    public class CommandHandler : AsyncDynamicRequestResponseCommandHandler
    {
        private readonly IAppUnitOfWork _appUnitOfWork;

        public CommandHandler(IAppUnitOfWork appUnitOfWork)
        {
            _appUnitOfWork = appUnitOfWork;
        }

        public async override Task<Result<object>> HandleAsync(string commandName, dynamic command, CancellationToken cancellationToken = default)
        {
            _appUnitOfWork.AuthorRepository.Add(new Author() { Name = "David Ikin" }, "");

            await _appUnitOfWork.CompleteAsync();

            return Result.Ok<object>(null);
        }
    }
}
