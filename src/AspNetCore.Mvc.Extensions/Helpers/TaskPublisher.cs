using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    //https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.PublishStrategies/PublishStrategy.cs
    public static class TaskPublisher
    {
        //Run each notification handler after one another. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
        public static async Task SyncContinueOnException(this IEnumerable<Func<Task>> handlers)
        {
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler().ConfigureAwait(false);
                }
                catch (AggregateException ex)
                {
                    exceptions.AddRange(ex.Flatten().InnerExceptions);
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        //Run each notification handler after one another. Returns when all handlers are finished or an exception has been thrown. In case of an exception, any handlers after that will not be run.
        public static async Task SyncStopOnException(this IEnumerable<Func<Task>> handlers)
        {
            foreach (var handler in handlers)
            {
                await handler().ConfigureAwait(false);
            }
        }

        // Run all notification handlers asynchronously. Returns when all handlers are finished. In case of any exception(s), they will be captured in an AggregateException.
        public static async Task AsyncContinueOnException(this IEnumerable<Func<Task>> handlers)
        {
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    tasks.Add(handler());
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                exceptions.Add(ex);
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        //Run each notification handler on it's own thread using Task.Run(). Returns immediately and does not wait for any handlers to finish. Note that you cannot capture any exceptions, even if you await the call to Publish.
        public static Task ParallelNoWait(this IEnumerable<Func<Task>> handlers)
        {
            foreach (var handler in handlers)
            {
                Task.Run(() => handler());
            }

            return Task.CompletedTask;
        }


        //Run each notification handler on it's own thread using Task.Run(). Returns when any thread (handler) is finished. Note that you cannot capture any exceptions (See msdn documentation of Task.WhenAny)
        public static Task ParallelWhenAny(this IEnumerable<Func<Task>> handlers)
        {
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                tasks.Add(Task.Run(() => handler()));
            }

            return Task.WhenAny(tasks);
        }

        //Run each notification handler on it's own thread using Task.Run(). Returns when all threads (handlers) are finished. In case of any exception(s), they are captured in an AggregateException by Task.WhenAll.
        public static Task ParallelWhenAll(this IEnumerable<Func<Task>> handlers)
        {
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                tasks.Add(Task.Run(() => handler()));
            }

            return Task.WhenAll(tasks);
        }

    }
}
