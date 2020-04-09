﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ApiClient
{
    //When a request times out a TaskCanceledException is thrown.
    //This throws a TimeOutException instead which allows for differentiation
    public class TimeOutDelegatingHttpHandler : DelegatingHandler
    {
        private readonly TimeSpan _timeOut = TimeSpan.FromSeconds(100);

        public TimeOutDelegatingHttpHandler(TimeSpan timeOut)
            : base()
        {
            _timeOut = timeOut;
        }

        public TimeOutDelegatingHttpHandler(HttpMessageHandler innerHandler,
           TimeSpan timeOut)
        : base(innerHandler)
        {
            _timeOut = timeOut;
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var linkedCancellationTokenSource = 
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                linkedCancellationTokenSource.CancelAfter(_timeOut);
                try
                {
                    return await base.SendAsync(request, linkedCancellationTokenSource.Token);
                }
                catch (OperationCanceledException ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("The request timed out.", ex);
                    }
                    throw;
                }
            }
        }
    }
}
