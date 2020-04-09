using AspNetCore.Mvc.Extensions.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AspNetCore.Mvc.Extensions.Middleware.ImageProcessing
{
    public class ContentHandlerMiddleware
    {
        private readonly ContentHttpHandlerOptions _options;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly RequestDelegate _next;

        // Must have constructor with this signature, otherwise exception at run time
        public ContentHandlerMiddleware(RequestDelegate next, ContentHttpHandlerOptions options, IWebHostEnvironment hostingEnvironment)
        {
            _options = options;
            _hostingEnvironment = hostingEnvironment;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            return new ContentHttpHandler(_options, _hostingEnvironment).ProcessRequestAsync(context);
            // await _next.Invoke(context); 
        }
    }

    public class ContentHttpHandlerOptions
    {
        public List<string> ValidFolders { get; set; } = new List<string>();
        public int CacheDays { get; set; } = 0;

        public bool ImageWatermarkEnabled { get; set; } = false;
        public string ImageWatermark { get; set; } = "";
        public string ImageWatermarkMinHeight { get; set; } = "100";
        public string ImageWatermarkMinWidth{ get; set; } = "100";

        public string FFMpeg { get; set; } = "";
    }

    public class ContentHttpHandler : BaseRangeRequestHandler
    {
        private readonly ContentHttpHandlerOptions _options;
        private IWebHostEnvironment _hostingEnvironment;
        public ContentHttpHandler(ContentHttpHandlerOptions options, IWebHostEnvironment hostingEnvironment)
        {
            _options = options;
            _hostingEnvironment = hostingEnvironment;
        }

        private byte[] _responseBytes;
        private string _responseContentType;
        private string _responseFileName;
        private string _physicalFilePath;
        private Boolean _isValidRequest;
        private Boolean _isUnAuthorized;

        private int size;
        private int width;
        private int height;
        private int minwidth;
        private int minheight;
        private int maxwidth;
        private int maxheight;
        private bool crop;
        private Boolean download;
        private Boolean watermark;

        public override int CacheDays()
        {
            return _options.CacheDays;
        }

        public override int BufferSize => 4096;

        public override void ParseRequestParameters(HttpContext context)
        {
            var pathName = HttpUtility.UrlDecode(context.Request.Path.ToString());
            _physicalFilePath = _hostingEnvironment.MapWwwPath(pathName);
            if (_physicalFilePath != "")
            {
                _responseFileName = Path.GetFileName(_physicalFilePath);
                var folder = Path.GetDirectoryName(pathName).Replace("\\","/");
                if (context.User.IsInRole("admin") || _options.ValidFolders.Where(f => pathName.StartsWith(f)).Count() > 0)
                {
                    if (FileHelper.FileExists(_physicalFilePath))
                    {
                        _isValidRequest = true;
                    }
                    else
                    {
                        if (!FileHelper.DirectoryExists(_hostingEnvironment.MapWwwPath(folder)))
                        {
                            folder = folder.Replace("---", "%%%").Replace("-", " ").Replace("%%%", " - ");
                        }

                        var fileName = _responseFileName;
                        var pathNameWithoutDashes = folder + "/" + fileName.ToString().Replace("---", "%%%").Replace("-", " ").Replace("%%%", " - ");
                        _physicalFilePath = _hostingEnvironment.MapWwwPath(pathNameWithoutDashes);

                        if (FileHelper.FileExists(_physicalFilePath))
                        {
                            _isValidRequest = true;
                        }
                    }
                }
                else
                {
                    _isValidRequest = false;
                    _isUnAuthorized = true;
                }
            }

            if (_isValidRequest)
            {
                if (context.Request.Query.ContainsKey("height"))
                {
                    try
                    {
                        this.height = int.Parse(context.Request.Query["height"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("width"))
                {
                    try
                    {
                        this.width = int.Parse(context.Request.Query["width"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("minheight"))
                {
                    try
                    {
                        this.minheight = int.Parse(context.Request.Query["minheight"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("minwidth"))
                {
                    try
                    {
                        this.minwidth = int.Parse(context.Request.Query["minwidth"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("maxheight"))
                {
                    try
                    {
                        this.maxheight = int.Parse(context.Request.Query["maxheight"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("maxwidth"))
                {
                    try
                    {
                        this.maxwidth = int.Parse(context.Request.Query["maxwidth"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("size"))
                {
                    try
                    {
                        this.size = int.Parse(context.Request.Query["size"]);
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("crop"))
                {
                    try
                    {
                        this.crop = (context.Request.Query["crop"] == "Y");
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("download"))
                {
                    try
                    {
                        this.download = true;
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("watermark"))
                {
                    try
                    {
                        this.watermark = true;
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
                if (context.Request.Query.ContainsKey("thumb"))
                {
                    try
                    {
                        this.download = false;
                        if (this.size == 0 && this.width == 0 && this.height == 0)
                        {
                            this.size = 75;
                        }
                    }
                    catch (Exception)
                    {
                        _isValidRequest = false;
                    }
                }
            }
        }

        public async override Task CreateResponseContentIfRequiredAsync(HttpContext context)
        {
            _responseContentType = MimeMapping.GetMimeMapping(_physicalFilePath);

            byte[] bytes10 = await FileHelper.GetBytesAsync(_physicalFilePath, 10);

            if (this.width <= 0 && this.height <= 0 && this.size <= 0 && this.maxwidth == 0 && this.maxheight == 0)
            {
                if (ImageHelper.IsValidImage(bytes10))
                {
                    bool changesMade = false;
                    bool changesCanBeMade = false;

                    if (changesCanBeMade)
                    {
                        byte[] array = await FileHelper.GetBytesAsync(_physicalFilePath);
                        using (Image disposableImage = ImageHelper.GetImage(array))
                        {
                            Image image = disposableImage;
                            this.Rotate(image, ImageHelper.ImageFilterHelper.Rotate.None, ref changesMade);
                            this.EditImage(ref image, 0, 0, 0, 0, ref changesMade);
                            this.Filter(ref image, ImageHelper.ImageFilterHelper.Filters.None, ref changesMade);
                            //this.Watermark(ref image, fileInfoOutput, ref flag);

                            if (changesMade)
                            {
                                _responseBytes = ImageHelper.imageToPNGByteArray(image);
                                _responseFileName = Path.ChangeExtension(_responseFileName, ".png");
                                _responseContentType = "image/png";
                            }
                        }

                    }
                }
            }
            else
            {
                _responseContentType = "image/jpg";
                Image disposableImage = null;

                try
                {
                    bool changesMade = false;

                    if (ImageHelper.IsValidImage(bytes10))
                    {
                        byte[] bytes = await FileHelper.GetBytesAsync(_physicalFilePath);
                        disposableImage = ImageHelper.GetImage(bytes);
                        disposableImage = this.Rotate(disposableImage, ImageHelper.ImageFilterHelper.Rotate.None, ref changesMade);
                    }
                    else if (GetRequestedFileInfo(context).IsVideo())
                    {
                        var converter = new VideoConverter(_hostingEnvironment.MapContentPath(_options.FFMpeg));
                        disposableImage = converter.GetPreviewImage(_physicalFilePath).PreviewImage;
                    }
                    else
                    {
                        byte[] bytes256 = await FileHelper.GetBytesAsync(_physicalFilePath, 256);
                        disposableImage = FileHelper.GetFileIconImageFromFileNameAndBytes(_responseFileName, bytes256, true);
                    }

                    _responseFileName = this.width.ToString() + this.height.ToString() + this.size.ToString() + _responseFileName;

                    if ((disposableImage.Width > this.maxwidth && this.maxwidth != 0) || (disposableImage.Height > this.maxheight && this.maxheight != 0))
                    {
                        disposableImage = ImageHelper.ResizeImageMaxWidthHeightConstraint(disposableImage, this.maxwidth, this.maxheight, false, ImageHelper.CropOption.Centre);
                    }
                    else if (this.size > 0)
                    {
                        disposableImage = ImageHelper.ResizeImageWidthHeightConstraint(disposableImage, this.size, true, ImageHelper.CropOption.Centre);
                    }
                    else if (this.width > 0 && this.height > 0)
                    {
                        disposableImage = ImageHelper.ResizeImageWidthHeightConstraint(disposableImage, this.width, this.height, true, ImageHelper.CropOption.Centre);
                    }
                    else if (this.height > 0)
                    {
                        disposableImage = ImageHelper.ResizeImageHeightConstraint(disposableImage, this.height);
                    }
                    else if (this.width > 0)
                    {
                        disposableImage = ImageHelper.ResizeImageWidthConstraint(disposableImage, this.width);
                    }

                    disposableImage = this.EditImage(ref disposableImage, 0, 0, 0, 0, ref changesMade);
                    disposableImage = this.Filter(ref disposableImage, ImageHelper.ImageFilterHelper.Filters.None, ref changesMade);
                    disposableImage = this.Shape(ref disposableImage, ImageHelper.ImageFilterHelper.Shape.None, ref changesMade);
                    disposableImage = this.Watermark(ref disposableImage, ImageHelper.Watermark.TopRight, "", ref changesMade);

                    byte[] array2 = ImageHelper.imageToJPEGByteArray(disposableImage, (long)75);

                    _responseBytes = array2;
                    _responseFileName = Path.ChangeExtension(_responseFileName, ".jpg");
                }
                catch
                {

                }
                finally
                {
                    if (disposableImage != null)
                    {
                        disposableImage.Dispose();
                    }

                }

            }
        }

        public override bool RequestResourceExists(HttpContext context)
        {
            return _isValidRequest;
        }

        public override bool DownloadFile(HttpContext context)
        {
            return download;
        }

        public override string GetRequestedFileMimeType(HttpContext context)
        {
            return _responseContentType;
        }

        private Boolean _fileInfoLoaded;
        private FileInfo _fileInfo;
        public override FileInfo GetRequestedFileInfo(HttpContext context)
        {
            if (!_fileInfoLoaded)
            {
                if (File.Exists(_physicalFilePath))
                {
                    _fileInfo = new FileInfo(_physicalFilePath);
                }
                _fileInfoLoaded = true;
            }
            return _fileInfo;
        }

        public override Stream GetResponseStream(HttpContext context)
        {
            if (_responseBytes != null)
            {
                return new MemoryStream(_responseBytes);
            }
            else
            {
                return new FileStream(InternalRequestedFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize);
            }
        }

        public override string GetResponseFileName(HttpContext context)
        {
            return _responseFileName;
        }

        public override bool IsAuthorized(HttpContext context)
        {
            return !_isUnAuthorized;
        }

        protected override long GetResponseFileLength()
        {
            if (_responseBytes != null)
            {
                return _responseBytes.Length;
            }
            else
            {
                return InternalRequestedFileInfo.Length;
            }
        }

        #region Transformations
        public void Rotate(ref byte[] buffer, ImageHelper.ImageFilterHelper.Rotate rotate)
        {
            if (rotate != ImageHelper.ImageFilterHelper.Rotate.None && ImageHelper.IsValidImage(buffer))
            {
                Image image = ImageHelper.GetImage(buffer);
                Image arg_22_1 = image;
                bool flag = false;
                buffer = ImageHelper.imageToPNGByteArray(this.Rotate(arg_22_1, rotate, ref flag));
            }
        }

        public Bitmap Rotate(Image image, ImageHelper.ImageFilterHelper.Rotate rotate, ref bool applied)
        {
            if (rotate != ImageHelper.ImageFilterHelper.Rotate.None)
            {
                applied = true;
                return ImageHelper.ImageFilterHelper.ApplyRotate(image, rotate);
            }
            return (Bitmap)image;
        }

        public void EditImage(ref byte[] buffer, int brightness, int contrast, int gamma, int sharpen)
        {
            if (ImageHelper.IsValidImage(buffer))
            {
                Image image = ImageHelper.GetImage(buffer);
                bool flag = false;
                buffer = ImageHelper.imageToPNGByteArray(this.EditImage(ref image, brightness, contrast, gamma, sharpen, ref flag));
            }
        }

        public Bitmap EditImage(ref Image image, int brightness, int contrast, int gamma, int sharpen, ref bool applied)
        {
            if (brightness != 0L || contrast != 0L || gamma != 0L || sharpen != 0L)
            {
                applied = true;
                return checked(image.Brightness((int)brightness).Contrast((int)contrast).Gamma((int)gamma).Sharpen((int)sharpen));
            }
            return (Bitmap)image;
        }

        public void Filter(ref byte[] buffer, ImageHelper.ImageFilterHelper.Filters filter)
        {
            if (filter != ImageHelper.ImageFilterHelper.Filters.None && ImageHelper.IsValidImage(buffer))
            {
                Image image = ImageHelper.GetImage(buffer);
                bool flag = false;
                buffer = ImageHelper.imageToPNGByteArray(this.Filter(ref image, filter, ref flag));
            }
        }

        public Bitmap Filter(ref Image image, ImageHelper.ImageFilterHelper.Filters filter, ref bool applied)
        {
            if (filter != ImageHelper.ImageFilterHelper.Filters.None)
            {
                applied = true;
                return ImageHelper.ImageFilterHelper.ApplyFilter(image, filter);
            }
            return (Bitmap)image;
        }

        public void Watermark(ref byte[] buffer, ImageHelper.Watermark waterMark, string waterMarkString)
        {
            if (waterMark != ImageHelper.Watermark.None && ImageHelper.IsValidImage(buffer))
            {
                Image image = ImageHelper.GetImage(buffer);
                bool flag = false;
                buffer = ImageHelper.imageToPNGByteArray(this.Watermark(ref image, waterMark, waterMarkString, ref flag));
            }
        }

        public Bitmap Watermark(ref Image image, ImageHelper.Watermark waterMark, string waterMarkString, ref bool applied)
        {
            if (waterMark != ImageHelper.Watermark.None && (_options.ImageWatermarkEnabled))
            {
                return ImageHelper.ApplyWatermark(image, checked((ImageHelper.Watermark)waterMark), int.Parse(_options.ImageWatermarkMinHeight), int.Parse(_options.ImageWatermarkMinWidth), 10, 10, _options.ImageWatermark, ref applied);
            }
            else if (waterMark != ImageHelper.Watermark.None && watermark)
            {
                return ImageHelper.ApplyWatermark(image, checked((ImageHelper.Watermark)waterMark), 10, 10, _options.ImageWatermark, ref applied);
            }
            return (Bitmap)image;
        }

        public Bitmap Shape(ref Image image, ImageHelper.ImageFilterHelper.Shape shape, ref bool applied)
        {
            if (shape != ImageHelper.ImageFilterHelper.Shape.None)
            {
                applied = true;
                return ImageHelper.ImageFilterHelper.ApplyShape(image, checked((ImageHelper.ImageFilterHelper.Shape)shape));
            }
            return (Bitmap)image;
        }
        #endregion
    }
}
