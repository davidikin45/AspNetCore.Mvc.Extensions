using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Mapping
{
    //Use AutoMapper.Extensions.Microsoft.DependencyInjection
     //services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    public class DtoMapAttribute : ResultFilterAttribute
    {
        private readonly Type _destinationType;

        public DtoMapAttribute(Type destinationType)
        {
            _destinationType = destinationType;
        }

        public async override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var resultFromAction = context.Result as ObjectResult;
            if(resultFromAction?.Value == null || resultFromAction.StatusCode < 200 || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            var mapper = context.HttpContext.RequestServices.GetRequiredService<IMapper>();
            if (typeof(IEnumerable).IsAssignableFrom(resultFromAction.Value.GetType()))
            {
                resultFromAction.Value = mapper.Map(resultFromAction.Value, resultFromAction.Value.GetType(), typeof(IEnumerable<>).MakeGenericType(_destinationType));
            }
            else
            {
                resultFromAction.Value = mapper.Map(resultFromAction.Value, resultFromAction.Value.GetType(), _destinationType);
            }

            await next();
        }
    }
}
