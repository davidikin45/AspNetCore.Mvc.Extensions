﻿using AspNetCore.Mvc.Extensions.ActionResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Razor
{
    //https://blog.elmah.io/generate-a-pdf-from-asp-net-core-for-free/?fbclid=IwAR0PONCCu6U83mYUGhwhxG6eADag-IRwsAejgrcGtzpvd1npBclfESoA8IQ
    //https://wkhtmltopdf.org/ install > wkhtmltopdf/Windows/wkhtmltopdf.exe (Copy if newer)
    //Wkhtmltopdf.NetCore
    public class GeneratePdfService : IGeneratePdfService
    {
        private readonly IViewRenderService _engine;
        private readonly GeneratePdfOptions _options;

        public GeneratePdfService(IViewRenderService engine, IOptions<GeneratePdfOptions> options)
        {
            _engine = engine;
            _options = options.Value;
        }

        public byte[] GetPdf(string html)
        {
            return WkhtmlDriver.Convert(_options.wkhtmltopdfPath, _options.GetConvertOptions(), html);
        }

        public async Task<IActionResult> DisplayViewPdfAsync(string viewName, object model, object additionalViewData = null)
        {
            var byteArray = await GetViewPdfAsync(viewName, model, additionalViewData);
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;
            return new DisplayFileStreamResult(pdfStream, "application/pdf");
        }

        public async Task<IActionResult> DownloadViewPdfAsync(string fileDownloadName, string viewName, object model, object additionalViewData = null)
        {
            var byteArray = await GetViewPdfAsync(viewName, model, additionalViewData);
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;
            return new DownloadFileStreamResult(pdfStream, "application/pdf", fileDownloadName);
        }

        public async Task<byte[]> GetViewPdfAsync(string viewName, object model, object additionalViewData = null)
        {
            var html = await _engine.ViewAsync(viewName, model, additionalViewData);
            return GetPdf(html);
        }

        public async Task<IActionResult> DisplayViewHtmlPdfAsync(string viewHtml, object model, object additionalViewData = null)
        {
            var byteArray = await GetViewHtmlPdfAsync(viewHtml, model, additionalViewData);
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;
            return new DisplayFileStreamResult(pdfStream, "application/pdf");
        }

        public async Task<IActionResult> DownloadViewHtmlPdfAsync(string fileDownloadName, string viewHtml, object model, object additionalViewData = null)
        {
            var byteArray = await GetViewHtmlPdfAsync(viewHtml, model, additionalViewData);
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;
            return new DownloadFileStreamResult(pdfStream, "application/pdf", fileDownloadName);
        }

        public async Task<byte[]> GetViewHtmlPdfAsync(string viewHtml, object model, object additionalViewData = null)
        {
            var html = await _engine.ViewHtmlAsync(viewHtml, model, additionalViewData);
            return GetPdf(html);
        }

        public void AddView(string path, string viewHtml) => _engine.AddView(path, viewHtml);

        public bool ExistsView(string path) => _engine.ExistsView(path);

        public void UpdateView(string path, string viewHtml) => _engine.UpdateView(path, viewHtml);

        private class WkhtmlDriver
        {
            /// <summary>
            /// Converts given URL or HTML string to PDF.
            /// </summary>
            /// <param name="wkhtmlPath">Path to wkthmltopdf\wkthmltoimage.</param>
            /// <param name="switches">Switches that will be passed to wkhtmltopdf binary.</param>
            /// <param name="html">String containing HTML code that should be converted to PDF.</param>
            /// <returns>PDF as byte array.</returns>
            public static byte[] Convert(string wkhtmlPath, string switches, string html)
            {
                // switches:
                //     "-q"  - silent output, only errors - no progress messages
                //     " -"  - switch output to stdout
                //     "- -" - switch input to stdin and output to stdout
                switches = "-q " + switches + " -";

                // generate PDF from given HTML string, not from URL
                if (!string.IsNullOrEmpty(html))
                {
                    switches += " -";
                    html = SpecialCharsEncode(html);
                }

                string rotativaLocation;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    rotativaLocation = Path.Combine(wkhtmlPath, "Windows", "wkhtmltopdf.exe");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    rotativaLocation = Path.Combine(wkhtmlPath, "Mac", "wkhtmltopdf");
                }
                else
                {
                    rotativaLocation = Path.Combine(wkhtmlPath, "Linux", "wkhtmltopdf");
                }

                if (!File.Exists(rotativaLocation))
                {
                    throw new Exception("wkhtmltopdf not found, searched for " + rotativaLocation);
                }

                using (var proc = new Process())
                {
                    try
                    {
                        proc.StartInfo = new ProcessStartInfo
                        {
                            FileName = rotativaLocation,
                            Arguments = switches,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            CreateNoWindow = true
                        };

                        proc.Start();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    // generate PDF from given HTML string, not from URL
                    if (!string.IsNullOrEmpty(html))
                    {
                        using (var sIn = proc.StandardInput)
                        {
                            sIn.WriteLine(html);
                        }
                    }

                    using (var ms = new MemoryStream())
                    {
                        using (var sOut = proc.StandardOutput.BaseStream)
                        {
                            byte[] buffer = new byte[4096];
                            int read;

                            while ((read = sOut.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                        }

                        string error = proc.StandardError.ReadToEnd();

                        if (ms.Length == 0)
                        {
                            throw new Exception(error);
                        }

                        proc.WaitForExit();

                        return ms.ToArray();
                    }
                }
            }

            /// <summary>
            /// Encode all special chars
            /// </summary>
            /// <param name="text">Html text</param>
            /// <returns>Html with special chars encoded</returns>
            private static string SpecialCharsEncode(string text)
            {
                var chars = text.ToCharArray();
                var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

                foreach (var c in chars)
                {
                    var value = System.Convert.ToInt32(c);
                    if (value > 127)
                        result.AppendFormat("&#{0};", value);
                    else
                        result.Append(c);
                }

                return result.ToString();
            }
        }
    }
}
