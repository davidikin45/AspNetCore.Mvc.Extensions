using Microsoft.AspNetCore.Routing.Constraints;

namespace AspNetCore.Mvc.Extensions.Routing.Constraints
{
    public class CultureConstraint : RegexRouteConstraint
    {
        public CultureConstraint()
            : base(@"^[a-zA-Z]{2,3}(\-[a-zA-Z]{4})?(\-[a-zA-Z0-9]{2,3})?$") { }
    }
}
