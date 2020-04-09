using AspNetCore.Mvc.Extensions.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace TemplateAspNetCore3.Data
{
    public class AppContextDesignTimeFactory : DesignTimeDbContextFactoryBase<AppContext>
    {
        public AppContextDesignTimeFactory()
            : base("DefaultConnection", typeof(AppContext).GetTypeInfo().Assembly.GetName().Name)
        {
        }

        protected override AppContext CreateNewInstance(DbContextOptions<AppContext> options)
        {
            return new AppContext(options);
        }
    }
}
