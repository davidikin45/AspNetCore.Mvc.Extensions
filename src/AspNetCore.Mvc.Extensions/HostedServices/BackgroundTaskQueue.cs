using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
        void QueueBackgroundWorkItem(Func<IServiceProvider,CancellationToken, Task> workItem);
        void QueueBackgroundWorkItem<TScopedProcessingService>(params object[] parameters) where TScopedProcessingService : IScopedProcessingService;
        Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly IServiceProvider _serviceProvider;
        private ConcurrentQueue<Func<IServiceProvider,CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<IServiceProvider,CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public BackgroundTaskQueue(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            QueueBackgroundWorkItem((_, ct) => workItem(ct));
        }

        public void QueueBackgroundWorkItem(Func<IServiceProvider,CancellationToken, Task> workItem)
        {
            if(workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            
            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public void QueueBackgroundWorkItem<TBackgroundWorkItem>(params object[] parameters) where TBackgroundWorkItem : IScopedProcessingService
        {
            QueueBackgroundWorkItem((scopedServiceProvider, cancellationToken) =>
            {
                var workItem = ActivatorUtilities.CreateInstance<TBackgroundWorkItem>(scopedServiceProvider, parameters) as IScopedProcessingService;
                return workItem.ExecuteAsync(cancellationToken);
            });
        }

        public async Task<Func<IServiceProvider,CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

    }
}
