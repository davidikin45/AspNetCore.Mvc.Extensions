using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace AspNetCore.Mvc.Extensions.Attributes.Display
{
    public class FileImageAudioVideoAcceptAttribute : FileAcceptAttribute
    {
        public FileImageAudioVideoAcceptAttribute()
            : base("image/*,audio/*,video/*")
        {

        }
    }

    public class FileImageVideoAcceptAttribute : FileAcceptAttribute
    {
        public FileImageVideoAcceptAttribute()
            : base("image/*,video/*")
        {

        }
    }

    public class FileImageAcceptAttribute : FileAcceptAttribute
    {
        public FileImageAcceptAttribute()
            : base("image/*")
        {

        }
    }

    public class FileAudioAcceptAttribute : FileAcceptAttribute
    {
        public FileAudioAcceptAttribute()
            : base("audio/*")
        {

        }
    }

    public class FileVideoAcceptAttribute : FileAcceptAttribute
    {
        public FileVideoAcceptAttribute()
            : base("video/*")
        {

        }
    }

    public class FileAcceptAttribute : Attribute, IDisplayMetadataAttribute
    {
        public string Accept { get; set; }

        public FileAcceptAttribute(string accept)
        {
            Accept = accept;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["Accept"] = Accept;
        }
    }
}
