using AspNetCore.Mvc.Extensions.Validation.Settings;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class CacheSettings : IValidateSettings
    {
        public int UploadFilesDays { get; set; } = 7;
        public int VersionedStaticFilesDays { get; set; } = 30;
        public int NonVersionedStaticFilesDays { get; set; } = 7;
    }
}
