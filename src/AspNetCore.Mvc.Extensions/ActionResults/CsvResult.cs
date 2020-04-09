using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{
    public class CsvResult : IActionResult
    {
        private IEnumerable sourceData;
        private string name;
        private string columnSeparator;
        private Boolean includeHeader;

        public CsvResult(IEnumerable data, string fileName, Boolean includeHeader = true, string columnSeparator = ",")
        {
            this.sourceData = data;
            this.name = fileName;
            this.columnSeparator = columnSeparator;
            this.includeHeader = includeHeader;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            bool headerIncluded = false;

            foreach (var rate in sourceData)
            {
                var properties = rate.GetType().GetProperties();

                if (includeHeader && !headerIncluded)
                    writer.WriteLine(string.Join(columnSeparator, properties.Select(f => MakeValueCsvFriendly(f.Name, columnSeparator))));

                headerIncluded = true;

                writer.WriteLine(string.Join(columnSeparator, properties.Select(f => MakeValueCsvFriendly(FindPropertyValue(rate, f.Name), columnSeparator))));
            }

            var csvBytes = Encoding.ASCII.GetBytes(writer.ToString());


            context.HttpContext.Response.Headers["content-type"] = "text/csv";
            context.HttpContext.Response.Headers["content-disposition"] = "attachment; filename=" + name + ".csv";
            return context.HttpContext.Response.Body.WriteAsync(csvBytes, 0, csvBytes.Length);
        }

        private string FindPropertyValue(object item, string prop)
        {
            return item.GetType().GetProperty(prop).GetValue(item, null).ToString() ?? "";
        }

        private static string MakeValueCsvFriendly(object value, string columnSeparator = ",")
        {
            if (value == null) return "";
            if (value is INullable && ((INullable)value).IsNull) return "";
            if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString().Trim();

            if (output.Length > 30000) //cropping value for stupid Excel
                output = output.Substring(0, 30000);

            if (output.Contains(columnSeparator) || output.Contains("\"") || output.Contains("\n") || output.Contains("\r"))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            return output;
        }
    }

    public class CsvResult<T> : IActionResult
    {
        private IEnumerable<T> sourceData;
        private string name;
        private string columnSeparator;
        private Boolean includeHeader;

        public CsvResult(IEnumerable<T> data, string fileName, Boolean includeHeader = true, string columnSeparator = ",")
        {
            this.sourceData = data;
            this.name = fileName;
            this.columnSeparator = columnSeparator;
            this.includeHeader = includeHeader;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            if (includeHeader)
                writer.WriteLine(string.Join(columnSeparator, typeof(T).GetProperties().Select(f => MakeValueCsvFriendly(f.Name, columnSeparator))));

            if (sourceData.Count() > 0)
            {
                int i = 0;
                foreach (var rate in sourceData)
                {
                    var properties = rate.GetType().GetProperties();

                    if(i < sourceData.Count()  - 1)
                    {
                        writer.WriteLine(string.Join(columnSeparator, properties.Select(f => MakeValueCsvFriendly(FindPropertyValue(rate, f.Name), columnSeparator))));
                    }
                    else
                    {
                        writer.Write(string.Join(columnSeparator, properties.Select(f => MakeValueCsvFriendly(FindPropertyValue(rate, f.Name), columnSeparator))));
                    }

                    i++;
                }
            }

            var csvBytes = Encoding.ASCII.GetBytes(writer.ToString());

            context.HttpContext.Response.Headers["content-type"] = "text/csv";
            context.HttpContext.Response.Headers["content-disposition"] = "attachment; filename=" + name + ".csv";
            return context.HttpContext.Response.Body.WriteAsync(csvBytes, 0, csvBytes.Length);
        }

        private string FindPropertyValue(object item, string prop)
        {
            return item.GetType().GetProperty(prop).GetValue(item, null).ToString() ?? "";
        }

        private static string MakeValueCsvFriendly(object value, string columnSeparator = ",")
        {
            if (value == null) return "";
            if (value is INullable && ((INullable)value).IsNull) return "";
            if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString().Trim();

            if (output.Length > 30000) //cropping value for stupid Excel
                output = output.Substring(0, 30000);

            if (output.Contains(columnSeparator) || output.Contains("\"") || output.Contains("\n") || output.Contains("\r"))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            return output;
        }
    }
}
