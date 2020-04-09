using AspNetCore.Cqrs;
using AspNetCore.Mvc.Extensions.Authentication;
using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Cqrs;
using AspNetCore.Mvc.Extensions.Helpers;
using AspNetCore.Mvc.Extensions.UI;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + BasicAuthenticationDefaults.AuthenticationScheme)]
    public abstract class ApiControllerCqrsAuthorizeBase : ApiControllerBase
    {
        protected readonly ICqrsMediator _cqrsMediator;

        public ApiControllerCqrsAuthorizeBase(ControllerServicesContext context, ICqrsMediator cqrsMediator)
        : base(context)
        {
            _cqrsMediator = cqrsMediator;
        }

        #region Queries
        [CqrsAuthorize]
        [Route("queries/{type}")]
        [HttpGet]
        public virtual Task<IActionResult> QueryFromRoute([FromRoute] string type)
        {
            var payload = Request.Query;

            var action = new ActionDto()
            {
                Type = type,
                Payload = ToDynamic(payload)
            };

            return QueryFromJson(action);
        }

        private dynamic ToDynamic(IQueryCollection collection)
        {
            ExpandoObject expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;

            var keys = collection.Keys;

            foreach (var item in keys.ToDictionary(key => key, value => collection[value]))
            {
                dictionary.Add(item.Key, item.Value);
            }

            return expando;
        }

        private static Type[] GetQueryItemType(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return toCheck.GetGenericArguments();
                }
                toCheck = toCheck.BaseType;
            }
            return null;
        }

        //Sytstem.Text.Json.JsonElement
        //JObject
        [CqrsAuthorize]
        [Route("queries")]
        [HttpPost]
        public virtual async Task<IActionResult> QueryFromJson([FromBody] ActionDto action)
        {
            if (action == null)
            {
                return Error(Messages.RequestInvalid);
            }

            bool isQuery = false;
            if (_cqrsMediator.CqrsQuerySubscriptionManager.HasSubscriptionsForQuery(action.Type))
            {
                isQuery = true;
            }

            if (string.IsNullOrWhiteSpace(action.Type) || (!isQuery))
            {
                return Error(Messages.ActionInvalid);
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrors(ModelState);
            }

            Type payloadType;
            CancellationTokenSource cts;

            cts = TaskHelper.CreateLinkedCancellationTokenSource(ClientDisconnectedToken());
            payloadType = _cqrsMediator.CqrsQuerySubscriptionManager.GetQueryTypeByName(action.Type);

            //query
            if (payloadType != null)
            {

                string payloadString = JsonConvert.SerializeObject(action.Payload);
                dynamic typedPayload = JsonConvert.DeserializeObject(payloadString, payloadType);

                var queryItemTypes = GetQueryItemType(typeof(GetListQuery<>), payloadType);
                if(queryItemTypes != null)
                {
                    var queryItemType = queryItemTypes.First();
                    Dictionary<string, StringValues> queryCollection = UIHelper.ToDictionaryForFilter(action.Payload);
                    if (!UIHelper.ValidFilterFor(queryItemType, queryCollection))
                    {
                        return BadRequest(Messages.FiltersInvalid);
                    }

                    dynamic filter = UIHelper.GetFilter(queryItemType, queryCollection);
                    if(filter != null)
                    {
                        typedPayload.Where = filter;
                    }
                }

                var actionForValidation = new ActionDto<dynamic>()
                {
                    Type = action.Type,
                    Payload = typedPayload
                };

                if (!TryValidateModel(actionForValidation))
                {
                    return ValidationErrors(ModelState);
                }

                dynamic result = await _cqrsMediator.DispatchAsync(typedPayload, cts.Token);

                return FromResult(result);
            }
            else
            {
                //dynamic query
                string payloadString = JsonConvert.SerializeObject(action.Payload);
                Result<dynamic> result = await _cqrsMediator.DispatchQueryAsync(action.Type, payloadString, cts.Token);
                return FromResult(result);
            }
        }

        [AllowAnonymous]
        [Route("queries")]
        [HttpGet]
        public virtual IActionResult Queries()
        {
            return Ok(_cqrsMediator.CqrsQuerySubscriptionManager.GetSubscriptions().OrderBy(i => i.Key).Select(c => new { Type = c.Key, Payload = c.Value.QueryType != null ? Activator.CreateInstance(c.Value.QueryType) : null, Return = c.Value.ReturnType != null ? Activator.CreateInstance(c.Value.ReturnType) : null }));
        }
        #endregion

        #region Commands
        [CqrsAuthorize]
        [Route("commands")]
        [HttpPost]
        public virtual async Task<IActionResult> Command([FromBody] ActionDto action)
        {
            if (action == null)
            {
                return Error(Messages.RequestInvalid);
            }

            bool isCommand = false;
            if (_cqrsMediator.CqrsCommandSubscriptionManager.HasSubscriptionsForCommand(action.Type))
            {
                isCommand = true;
            }

            if (string.IsNullOrWhiteSpace(action.Type) || (!isCommand))
            {
                return Error(Messages.ActionInvalid);
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrors(ModelState);
            }

            Type payloadType;
            CancellationTokenSource cts;

            cts = TaskHelper.CreateNewCancellationTokenSource();
            payloadType = _cqrsMediator.CqrsCommandSubscriptionManager.GetCommandTypeByName(action.Type);

            //command
            if (payloadType != null)
            {
                string payloadString = JsonConvert.SerializeObject(action.Payload);
                dynamic typedPayload = JsonConvert.DeserializeObject(payloadString, payloadType);

                var actionForValidation = new ActionDto()
                {
                    Type = action.Type,
                    Payload = typedPayload
                };

                if (!TryValidateModel(actionForValidation))
                {
                    return ValidationErrors(ModelState);
                }

                dynamic result = await _cqrsMediator.DispatchAsync(typedPayload, cts.Token);

                return FromResult(result);
            }
            else
            {
                //dynamic command
                string payloadString = JsonConvert.SerializeObject(action.Payload);
                Result<dynamic> result = await _cqrsMediator.DispatchCommandAsync(action.Type, payloadString, cts.Token);
                return FromResult(result);
            }
        }

        [AllowAnonymous]
        [Route("commands")]
        [HttpGet]
        public virtual IActionResult Commands()
        {
            return Ok(_cqrsMediator.CqrsCommandSubscriptionManager.GetSubscriptions().OrderBy(i => i.Key).Select(c => new { Type = c.Key, Payload = c.Value.CommandType != null ? Activator.CreateInstance(c.Value.CommandType) : null, Return = c.Value.ReturnType != null ? Activator.CreateInstance(c.Value.ReturnType) : null }));
        }
        #endregion
    }
}

