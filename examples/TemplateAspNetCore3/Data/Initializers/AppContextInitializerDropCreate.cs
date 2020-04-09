using AspNetCore.Mvc.Extensions.Data.Initializers;
using System.Threading.Tasks;
using TemplateAspNetCore3.Data;

namespace TemplateAspNetCore3.Data.Initializers
{
    public class AppContextInitializerDropCreate : ContextInitializerDropCreate<AppContext>
    {
        public override void Seed(AppContext context, string tenantId)
        {
            context.Seed();
        }

        public override Task OnSeedCompleteAsync(AppContext context)
        {
            return Task.CompletedTask; 
        }
    }
}
