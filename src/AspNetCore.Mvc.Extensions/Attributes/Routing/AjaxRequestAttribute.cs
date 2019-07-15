using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Routing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AcceptAjaxRequestAttribute : ActionMethodSelectorAttribute
    {
        #region Properties  
        public bool AcceptRequest { get; private set; }
        #endregion

        #region Constructor  
        public AcceptAjaxRequestAttribute(bool accept)
        {
            AcceptRequest = accept;
        }
        #endregion

        #region ActionMethodSelectorAttribute Members  

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (routeContext == null)
                throw new ArgumentNullException("routeContext");

            bool isAjaxRequest = ((routeContext.HttpContext.Request.Headers != null) && (routeContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"));

            return AcceptRequest == isAjaxRequest;
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AjaxRequestAttribute : ActionMethodSelectorAttribute
    {
        #region Fields  
        private static readonly AcceptAjaxRequestAttribute _innerAttribute = new AcceptAjaxRequestAttribute(true);
        #endregion

        #region ActionMethodSelectorAttribute Members  
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            return _innerAttribute.IsValidForRequest(routeContext, action);
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoAjaxRequestAttribute : ActionMethodSelectorAttribute
    {
        #region Fields  
        private static readonly AcceptAjaxRequestAttribute _innerAttribute = new AcceptAjaxRequestAttribute(false);
        #endregion

        #region ActionMethodSelectorAttribute Members  
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            return _innerAttribute.IsValidForRequest(routeContext, action);
        }
        #endregion
    }
}
