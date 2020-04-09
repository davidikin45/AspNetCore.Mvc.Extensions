using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.UnitOfWork
{
    public static class UnitOfWorkServiceProviderExtensions
    {
        public static void BeginUnitOfWork(this IServiceProvider serviceProvider)
        {
            var uows = serviceProvider.GetServices<IUnitOfWork>();
            uows.ToList().ForEach(uow => uow.CommitingChanges = true);
        }

        public static async Task CompleteUnitOfWorkAsync(this IServiceProvider serviceProvider)
        {
            var uows = serviceProvider.GetServices<IUnitOfWork>();
            uows.ToList().ForEach(uow => uow.CommitingChanges = false);

            foreach (var uow in uows)
            {
                await uow.CompleteAsync();
            }
        }
    }
}
