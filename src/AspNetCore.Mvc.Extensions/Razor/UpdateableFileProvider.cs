using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace AspNetCore.Mvc.Extensions.Razor
{
    public class UpdateableFileProvider : IFileProvider
    {
        public Dictionary<string, ViewFileInfo> Views = new Dictionary<string, ViewFileInfo>()
        {
            {
                "/Views/FakeView.cshtml",
                new ViewFileInfo(string.Empty)
            }
        };

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new NotFoundDirectoryContents();
        }

        public void UpdateContent(string content, string view = null)
        {
            var viewPath = string.IsNullOrWhiteSpace(view) ? "/Views/FakeView.cshtml" : view;
            var old = Views[viewPath];
            if (old.Content != content)
            {
                old.TokenSource.Cancel();
                Views[viewPath] = new ViewFileInfo(content);
            }
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var viewPath = string.IsNullOrWhiteSpace(subpath) ? "/Views/FakeView.cshtml" : subpath;
            if (!Views.TryGetValue(viewPath, out var fileInfo))
            {
                fileInfo = new ViewFileInfo(null);
            }

            return fileInfo;
        }

        public IChangeToken Watch(string filter)
        {
            if (Views.TryGetValue(filter, out var fileInfo))
            {
                return fileInfo.ChangeToken;
            }

            return NullChangeToken.Singleton;
        }
    }

    public class ViewFileInfo : IFileInfo
    {
        public readonly string Content;

        public ViewFileInfo(string content)
        {
            Content = content;
            ChangeToken = new CancellationChangeToken(TokenSource.Token);
            Exists = Content != null;
        }

        public bool Exists { get; }
        public bool IsDirectory => false;
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;
        public long Length => -1;
        public string Name { get; set; }
        public string PhysicalPath => null;
        public CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();
        public CancellationChangeToken ChangeToken { get; }

        public Stream CreateReadStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(Content));
        }
    }
}
