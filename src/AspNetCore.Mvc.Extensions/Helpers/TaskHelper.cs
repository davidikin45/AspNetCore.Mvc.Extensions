using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class TaskHelper
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        }

        public static async Task DoAsync(this Func<Task> action,
                                         AsyncRetryPolicy policy = null,
                                         CancellationToken? cancellationToken = null)
        {
            await DoAsync(async () =>
            {
                await action();
                return true;
            },
            policy, cancellationToken);
        }

        public static async Task<T> DoAsync<T>(this Func<Task<T>> action,
                                               AsyncRetryPolicy policy = null,
                                               CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;
            policy = policy ?? new AsyncRetryPolicy();


            for (var retry = 0; retry < policy.RetryCount; retry++)
            {
                try
                {
                    cancellationToken.Value.ThrowIfCancellationRequested();

                    return await action();
                }
                catch (Exception ex) when (!(ex is OperationCanceledException) && policy.ExceptionFilter(ex))
                {

                    if (retry < policy.RetryCount - 1)
                    {
                        try
                        {
                            policy.OnExceptionAction(ex);
                        }
                        catch (Exception)
                        {

                        }

                        await Task.Delay(policy.RetryInterval, cancellationToken.Value);
                    }
                    else
                    {
                        throw ex;
                    }
                }

            }
            throw new Exception("");
        }

        public static bool WaitAll(this Task[] tasks, int timeout, CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            foreach (var task in tasks)
            {
                task.ContinueWith(t => {
                    if (t.IsFaulted) cts.Cancel();
                },
                cts.Token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
            }

            return Task.WaitAll(tasks, timeout, cts.Token);
        }

        public static CancellationTokenSource CreateNewCancellationTokenSource()
        {
            var cts = new CancellationTokenSource();
            return cts;
        }

        public static CancellationTokenSource CreateChildCancellationTokenSource(params CancellationToken[] tokens)
        {
            var cts = CreateLinkedCancellationTokenSource(tokens);
            return cts;
        }

        public static CancellationTokenSource CreateLinkedCancellationTokenSource(params CancellationToken[] tokens)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(tokens);
            return cts;
        }

        public static CancellationToken CreateLinkedCancellationToken(params CancellationToken[] tokens)
        {
            var cts = CreateLinkedCancellationTokenSource(tokens);
            return cts.Token;
        }


        public static Task AsTask(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Object>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }

        public static Task<TResult[]> WhenAllOrException<TResult>(this IEnumerable<Task<TResult>> tasks, CancellationTokenSource cancellationTokenSource)
        {

            foreach (var task in tasks)
            {
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        cancellationTokenSource.Cancel();
                    }
                },
                cancellationTokenSource.Token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
            }

            return Task.WhenAll(tasks);
        }

        public static Task WhenAllOrException(CancellationTokenSource cancellationTokenSource, params Task[] tasks)
        {

            foreach (var task in tasks)
            {
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        cancellationTokenSource.Cancel();
                    }
                },
                cancellationTokenSource.Token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
            }

            return Task.WhenAll(tasks);
        }

    }
    public class AsyncRetryPolicy
    {
        public AsyncRetryPolicy(TimeSpan? retryInterval = null,
                                int retryCount = 0,
                                Func<Exception, bool> exceptionFilter = null,
                                Action<Exception> onExceptionAction = null)
        {
            RetryInterval = retryInterval ?? TimeSpan.FromSeconds(3);
            RetryCount = retryCount;
            ExceptionFilter = exceptionFilter ?? (ex => true);
            OnExceptionAction = onExceptionAction ?? (ex => { });
        }

        public TimeSpan RetryInterval { get; private set; }
        public int RetryCount { get; private set; }

        public Func<Exception, bool> ExceptionFilter { get; private set; }
        public Action<Exception> OnExceptionAction { get; private set; }
    }
}
