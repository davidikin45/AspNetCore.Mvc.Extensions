namespace AspNetCore.Mvc.Extensions.Authorization.Attributes
{
    public class ScopeAuthorizeAttribute : ClaimAuthorizeAttribute
    {
        public ScopeAuthorizeAttribute(params string[] allowedValues) : base("scope", allowedValues)
        {

        }
    }
}
