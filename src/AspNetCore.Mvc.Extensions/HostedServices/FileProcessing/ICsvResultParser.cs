using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AspNetCore.Mvc.Extensions.HostedServices.FileProcessing
{
    public interface ICsvResultParser<T>
    {
        IReadOnlyCollection<T> ParseResult(Stream stream);
    }
}
