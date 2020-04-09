using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.HostedServices.FileProcessing
{
    public interface IResultProcessor<T>
    {
        Task ProcessAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
