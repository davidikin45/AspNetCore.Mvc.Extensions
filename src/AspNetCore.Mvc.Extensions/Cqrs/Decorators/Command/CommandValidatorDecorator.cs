﻿using AspNetCore.Mvc.Extensions.Validation;
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

        public CommandValidatorDecorator(ICommandHandler<TCommand, TResult> handler, IServiceProvider serviceProvider)
        {
            _handler = handler;
            _serviceProvider = serviceProvider;
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
            var context = new ValidationContext(o, _serviceProvider, new Dictionary<object, object>());

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                context.ObjectInstance,
                context,
               validationResults,
               validateAllProperties: true); // if true [Required] + Other attributes

            return validationResults.Where(r => r != ValidationResult.Success);
        }
    }
}