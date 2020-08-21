using AspNetCore.Mvc.Extensions.Validation;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Cqrs.Decorators.Command
{
    public sealed class CommandValidatorDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> _handler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IValidator<TCommand>> _validators;

        public CommandValidatorDecorator(ICommandHandler<TCommand, TResult> handler, IServiceProvider serviceProvider, IEnumerable<IValidator<TCommand>> validators)
        {
            _handler = handler;
            _serviceProvider = serviceProvider;
            _validators = validators;
        }

        public Task<Result<TResult>> HandleAsync(string commandName, TCommand command, CancellationToken cancellationToken = default)
        {
            var validationResults = Validate(command).ToList();

            if (validationResults.Count > 0)
            {
                return Task.FromResult(Result.ObjectValidationFail<TResult>(validationResults));
            }

            return _handler.HandleAsync(commandName, command, cancellationToken);
        }

        //1. [Required]
        //2. Other attributes
        //3. IValidatableObject Implementation
        public IEnumerable<ValidationResult> Validate(object o)
        {
            var attributeValidationContext = new System.ComponentModel.DataAnnotations.ValidationContext(o, _serviceProvider, new Dictionary<object, object>());

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                attributeValidationContext.ObjectInstance,
                attributeValidationContext,
               validationResults,
               validateAllProperties: true); // if true [Required] + Other attributes

            var attributeValidationResults = validationResults.Where(r => r != ValidationResult.Success);

            var fluentValidationContext = new FluentValidation.ValidationContext(o);

            var fluentValidationFailures = _validators
                .Select(v => v.Validate(fluentValidationContext))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            var fluentValidationResults = fluentValidationFailures.Select(f => new ValidationResult(f.ErrorMessage, new string[] { f.PropertyName }));

            return attributeValidationResults.Concat(fluentValidationResults);
        }
    }
}
