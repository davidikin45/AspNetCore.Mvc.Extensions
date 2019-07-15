using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace AspNetCore.Mvc.Extensions.Routing.Constraints
{
    public class PromoConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            int letterCount = 0;
            int numCount = 0;
            int sum = 0;

            foreach (var unit in values["token"].ToString())
            {
                letterCount += Char.IsLetter(unit) ? 1 : 0;
                numCount += Char.IsDigit(unit) ? 0 : 1;
                sum += Char.IsDigit(unit) ? int.Parse(unit.ToString()) : 0;
            }

            return letterCount == 3 && numCount == 3 && sum % 3 == 0;
        }
    }
}
