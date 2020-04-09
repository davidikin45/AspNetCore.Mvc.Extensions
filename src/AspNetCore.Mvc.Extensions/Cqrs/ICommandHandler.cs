using AspNetCore.Mvc.Extensions.Cqrs;
using AspNetCore.Mvc.Extensions.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs
{
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default);
    }

    public interface ITypedCommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
    {

    }

    public interface ITypedCommandHandler<in TCommand> : ITypedCommandHandler<TCommand, string>
     where TCommand : ICommand<string>
    {

    }

    public interface IDynamicCommandHandler<in TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {

    }

    public interface IDynamicCommandHandler<in TCommand> : IDynamicCommandHandler<TCommand, string>
    {

    }
}
