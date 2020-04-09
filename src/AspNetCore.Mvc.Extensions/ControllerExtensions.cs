using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions
{
    public static class ControllerExtensions
    {
        public static IEnumerable<T> GetCustomAttributes<T>(this Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor) where T : Attribute
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                return controllerActionDescriptor.MethodInfo.ReflectedType.GetCustomAttributes(typeof(T), true).Select(a => (T)a);
            }

            return Enumerable.Empty<T>();
        }

        public static void AddValidationErrors(this ModelStateDictionary modelState, IEnumerable<ValidationResult> errors)
        {
            foreach (var err in errors)
            {
                if (err.MemberNames.Count() > 0)
                {
                    foreach (var prop in err.MemberNames)
                    {
                        modelState.AddModelError(prop, err.ErrorMessage);
                    }
                }
                else
                {
                    modelState.AddModelError("", err.ErrorMessage);
                }
            }
        }

        public static RedirectToRouteResult RedirectToAction<TController>(this TController controller, Expression<Action<TController>> action) where TController : Controller
        {
            return RedirectToAction((Controller)controller, action);
        }

        public static RedirectToRouteResult RedirectToAction<TController>(this Controller controller, Expression<Action<TController>> action) where TController : Controller
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            var result = AspNetCore.Mvc.Extensions.Helpers.ExpressionHelper.GetRouteValuesFromExpression(action);
            return new RedirectToRouteResult(result.RouteValues);
        }
    }
}
