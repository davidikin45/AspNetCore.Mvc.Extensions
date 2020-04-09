using AspNetCore.Mvc.Extensions.Validation.Settings;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Settings
{
    public class AppSettings : IValidateSettings
    {
        [Required]
        public string AssemblyPrefix { get; set; }
        public string MvcImplementationFolder { get; set; } = "Controllers/";
        public string ActiveViewTheme { get; set; } = "";
        public string SignalRUrlPrefix { get; set; } = "/api";
        public string CookieConsentName { get; set; } = "ConsentCookie";
        public string CookieAuthName { get; set; } = "AuthCookie";
        public string CookieApplicationAuthName { get; set; } = "ApplicationAuthCookie";
        public string CookieExternalAuthName { get; set; } = "ExternalAuthCookie";
        public string CookieTempDataName { get; set; } = "TempDataCookie";
        public int ResponseCacheSizeMB { get; set; } = 500;

        [Required]
        public string Timezone { get; set; } = "Pacific Standard Time";

        [Required]
        public string SiteTitle { get; set; }
        public bool AngularApp { get; set; } = false;
        public string TitleSeparator { get; set; } = "|";

        [Required]
        public string SiteDescription { get; set; }

        public string SiteLogoLarge { get; set; }
        public string SiteLogoSmall { get; set; }
        public string SiteShareImage { get; set; }
        public string SiteAboutMeImage { get; set; }
        public string SiteFooterImage { get; set; }
        public bool ImageWatermarkShareEnabled { get; set; } = false;
        public bool ImageWatermarkEnabled { get; set; } = false;
        public string ImageWatermark { get; set; }
        public string ImageWatermarkMinWidth { get; set; } = "700";
        public string ImageWatermarkMinHeight { get; set; } = "700";

        [Required]
        public string SiteKeyWords { get; set; }

        [Required]
        public string SiteAuthor { get; set; }
        public string SiteUrl { get; set; }
        [Required]
        public string Domain { get; set; }
        public string BodyFont { get; set; } = "Montserrat";
        public string NavBarFont { get; set; } = "Amatic SC";
        public string NavBarFontStyleCSS { get; set; }
        public string FacebookAppId { get; set; }
        public string DisqusShortName { get; set; }
        public string AddThisId { get; set; }
        public string GoogleMapsApiKey { get; set; }
        public string InstagramUserId { get; set; }
        public string InstagramAccessToken { get; set; }
        public string GoogleAnalyticsTrackingId { get; set; }
        public string GoogleAdSenseId { get; set; }
        public bool RSSFeed { get; set; } = false;
        public string GitHubLink { get; set; }
        public string InstagramLink { get; set; }
        public string FacebookLink { get; set; }
        public string LinkedInLink { get; set; }
        public string YouTubeLink { get; set; }
        public string PublicUploadFolders { get; set; }
        public string FFMpeg { get; set; } = "~/ffmpeg/";
        public string FFMpeg_ExeLocation { get; set; }
        public string FFMpeg_WorkingPath { get; set; }
        public int NumberOfDatabaseRetries { get; set; } = 3;
        public Dictionary<string, string> Folders { get; set; } = new Dictionary<string, string>();
        public string SwaggerUIUsername { get; set; } = "";
        public string SwaggerUIPassword { get; set; } = "";
    }
}
