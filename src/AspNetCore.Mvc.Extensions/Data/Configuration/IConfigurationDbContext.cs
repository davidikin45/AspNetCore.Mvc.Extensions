using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Mvc.Extensions.Data.Configuration
{
    public interface IConfigurationDbContext
    {
        DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }
    }
}
