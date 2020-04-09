using AspNetCore.Mvc.Extensions.Data.NoSql.Initializers;

namespace TemplateAspNetCore3.Data.Initializers
{
    public class NoSqlContextInitializerDropCreate : ContextInitializerNoSqlDropCreate<NoSqlContext>
    {
        public override void Seed(NoSqlContext context, string tenantId)
        {
            context.Seed();
        }
    }
}
