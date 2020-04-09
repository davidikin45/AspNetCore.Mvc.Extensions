using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class ExpressionHelper
    {

        public static PropertyInfo GetPropertyInfo<TSource, TValue>(
            this Expression<Func<TSource, TValue>> expression)
        {
            return (PropertyInfo)((MemberExpression)expression.Body).Member;
        }

        public static (string Action, string Controller, RouteValueDictionary RouteValues) GetRouteValuesFromExpression<TController>(Expression<Action<TController>> action) where TController : ControllerBase
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            MethodCallExpression call = action.Body as MethodCallExpression;
            if (call == null)
            {
                throw new ArgumentException("action");
            }

            string controllerName = typeof(TController).Name;
            if (!controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("action");
            }
            controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
            if (controllerName.Length == 0)
            {
                throw new ArgumentException("action");
            }

            // TODO: How do we know that this method is even web callable?
            //      For now, we just let the call itself throw an exception.

            string actionName = GetTargetActionName(call.Method);

            var rvd = new RouteValueDictionary();
            rvd.Add("Controller", controllerName);
            rvd.Add("Action", actionName);

            AddParameterValuesFromExpressionToDictionary(rvd, call);
            return (actionName, controllerName, rvd);
        }

        // This method contains some heuristics that will help determine the correct action name from a given MethodInfo
        // assuming the default sync / async invokers are in use. The logic's not foolproof, but it should be good enough
        // for most uses.
        private static string GetTargetActionName(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;

            // do we know this not to be an action?
            if (methodInfo.IsDefined(typeof(NonActionAttribute), true /* inherit */))
            {
                throw new InvalidOperationException(methodName);
            }

            // has this been renamed?
            ActionNameAttribute nameAttr = methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true /* inherit */).OfType<ActionNameAttribute>().FirstOrDefault();
            if (nameAttr != null)
            {
                return nameAttr.Name;
            }

            // targeting an async action?
            if (methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
            {
                return methodName.Substring(0, methodName.Length - "Async".Length);
            }
            if (methodName.EndsWith("Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(methodName);
            }

            // fallback
            return methodName;
        }

        private static void AddParameterValuesFromExpressionToDictionary(RouteValueDictionary rvd, MethodCallExpression call)
        {
            ParameterInfo[] parameters = call.Method.GetParameters();

            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression arg = call.Arguments[i];
                    object value = null;
                    ConstantExpression ce = arg as ConstantExpression;
                    if (ce != null)
                    {
                        // If argument is a constant expression, just get the value
                        value = ce.Value;
                    }
                    else
                    {
                        value = Expression.Lambda(arg).Compile().DynamicInvoke();
                    }
                    rvd.Add(parameters[i].Name, value);
                }
            }
        }

    }
}
