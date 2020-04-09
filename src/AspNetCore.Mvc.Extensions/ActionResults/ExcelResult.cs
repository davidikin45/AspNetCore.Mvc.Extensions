using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ActionResults
{
    public class ExcelResult : IActionResult
    {
        private IEnumerable sourceData;
        private string name;
        private Boolean includeHeader;

        public ExcelResult(IEnumerable data, string fileName, Boolean includeHeader = true)
        {
            this.sourceData = data;
            this.name = fileName;
            this.includeHeader = includeHeader;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(name);
                var currentRow = 1;

                var headerIncluded = false;
                foreach (var rate in sourceData)
                {
                    var properties = rate.GetType().GetProperties();

                    if (includeHeader && !headerIncluded)
                    {
                        for (int i = 0; i < properties.Length; i++)
                        {
                            worksheet.Cell(currentRow, i+1).Value = MakeValueExcelFriendly(properties[i].Name);
                        }
                        currentRow++;
                    }

                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = MakeValueExcelFriendly(FindPropertyValue(rate, properties[i].Name));
                    }

                    currentRow++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var bytes = stream.ToArray();

                    context.HttpContext.Response.Headers["content-type"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    context.HttpContext.Response.Headers["content-disposition"] = "attachment; filename=" + name + ".xlsx";
                    return context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
            }
        }

        private string FindPropertyValue(object item, string prop)
        {
            return item.GetType().GetProperty(prop).GetValue(item, null).ToString() ?? "";
        }

        private static string MakeValueExcelFriendly(object value, string columnSeparator = ",")
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

            return output;
        }
    }

    public class ExcelResult<T> : IActionResult
    {
        private IEnumerable<T> sourceData;
        private string name;
        private string columnSeparator;
        private Boolean includeHeader;

        public ExcelResult(IEnumerable<T> data, string fileName, Boolean includeHeader = true, string columnSeparator = ",")
        {
            this.sourceData = data;
            this.name = fileName;
            this.columnSeparator = columnSeparator;
            this.includeHeader = includeHeader;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                var currentRow = 1;

                var properties = typeof(T).GetProperties();
                if (includeHeader)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = MakeValueExcelFriendly(properties[i].Name);
                    }
                    currentRow++;
                }

                foreach (var rate in sourceData)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = MakeValueExcelFriendly(FindPropertyValue(rate, properties[i].Name));
                    }

                    currentRow++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var bytes = stream.ToArray();

                    context.HttpContext.Response.Headers["content-type"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    context.HttpContext.Response.Headers["content-disposition"] = "attachment; filename=" + name + ".xlsx";
                    return context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
            }
        }

        private string FindPropertyValue(object item, string prop)
        {
            return item.GetType().GetProperty(prop).GetValue(item, null).ToString() ?? "";
        }

        private static string MakeValueExcelFriendly(object value, string columnSeparator = ",")
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

            return output;
        }
    }
}
