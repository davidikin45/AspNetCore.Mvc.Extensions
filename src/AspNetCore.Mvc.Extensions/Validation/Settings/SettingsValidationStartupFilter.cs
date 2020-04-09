using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Validation.Settings
{
    public class SettingsValidationStartupFilter : IStartupFilter
    {
        readonly IEnumerable<IValidateSettings> _settings;
        public SettingsValidationStartupFilter(IEnumerable<IValidateSettings> settings)
        {
            _settings = settings;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            foreach (var setting in _settings)
            {
                var configErrors = ValidationErrors(setting).ToArray();
                if (!configErrors.Any())
                {
                    continue;
                }

                var aggregatedErrors = string.Join(",", configErrors);
                var count = configErrors.Length;
                var configType = setting.GetType().FullName;
                throw new Exception($"{configType} configuration has {count} error(s): {aggregatedErrors}");
            }

            //don't alter the configuration
            return next;
        }


        private static IEnumerable<string> ValidationErrors(object obj)
        {
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(obj, context, results, true);
            foreach (var validationResult in results.Where(r => r != ValidationResult.Success))
            {
                yield return validationResult.ErrorMessage;
            }
        }
    }
}
