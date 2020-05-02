using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data
{
    //https://gunnarpeipman.com/ef-core-dbcontext-repository/
    //UOW = Repository Container
    public interface IDbContext
    {
        //DbSet<Employer> Employers { get; set; }
        //ICustomerRepository EmployerQueries { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}