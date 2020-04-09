using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.HostedServices.FileProcessing
{
    public class CsvResultParser<T> : ICsvResultParser<T>
    {
        public IReadOnlyCollection<T> ParseResult(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<T>();

            return records.ToArray();
        }
    }
}
