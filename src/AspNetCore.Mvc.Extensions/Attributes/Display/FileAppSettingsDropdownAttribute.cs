using AspNetCore.Mvc.Extensions.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class FileAppSettingsDropdownAttribute : DropdownFileAttribute
    {
        public string FileFolderId { get; set; }
        public FileAppSettingsDropdownAttribute(string folderId, bool nullable = false)
            : base("")
        {
            FileFolderId = folderId;
            Nullable = nullable;
        }

        public override void TransformMetadata(DisplayMetadataProviderContext context, IServiceProvider serviceProvider)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            Path = appSettings.Folders[FileFolderId];
            base.TransformMetadata(context, serviceProvider);


        }
    }
}
