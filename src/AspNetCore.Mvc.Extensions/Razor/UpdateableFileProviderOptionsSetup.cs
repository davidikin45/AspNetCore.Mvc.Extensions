using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Options;

namespace AspNetCore.Mvc.Extensions.Razor
{
    public class UpdateableFileProviderMvcRazorRuntimeCompilationOptionsSetup : IConfigureOptions<MvcRazorRuntimeCompilationOptions>
    {
        private readonly UpdateableFileProvider _fileProvider;
        public UpdateableFileProviderMvcRazorRuntimeCompilationOptionsSetup(UpdateableFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }
        public void Configure(MvcRazorRuntimeCompilationOptions options)
        {
            options.FileProviders.Add(_fileProvider);
        }
    }

    //.NET Core 2.2 
    //public class UpdateableFileProviderRazorViewEngineOptionssSetup : IConfigureOptions<RazorViewEngineOptions>
    //{
    //    private readonly UpdateableFileProvider _fileProvider;
    //    public UpdateableFileProviderRazorViewEngineOptionssSetup(UpdateableFileProvider fileProvider)
    //    {
    //        _fileProvider = fileProvider;
    //    }
    //    public void Configure(RazorViewEngineOptions options)
    //    {
    //        options.FileProviders.Add(_fileProvider);
    //    }
    //}

}
