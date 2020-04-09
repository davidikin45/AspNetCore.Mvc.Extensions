using AspNetCore.Mvc.Extensions.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Features
{
    public class FeatureAttribute : TypeFilterAttribute
    {
        public FeatureAttribute(string feature)
            :base(typeof(FeatureAuthFilter))
        {
            Arguments = new object[] { feature };
        }
    }
}