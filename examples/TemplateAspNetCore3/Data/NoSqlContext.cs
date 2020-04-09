using AspNetCore.Mvc.Extensions.Data.NoSql;
using System.IO;

namespace TemplateAspNetCore3.Data
{
    public class NoSqlContext : DbContextNoSql
    {
        public NoSqlContext(string connectionString)
            : base(connectionString)
        {

        }

        public NoSqlContext(MemoryStream memoryStream)
           : base(memoryStream)
        {

        }

        public override void Seed()
        {

        }
    }
}
