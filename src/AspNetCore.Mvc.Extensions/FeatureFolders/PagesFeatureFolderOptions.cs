using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.FeatureFolders
{
    public class PagesFeatureFolderOptions
    {
        private string _rootFeatureFolder = "/Controllers";
        public string RootFeatureFolder
        {
            get
            {
                return _rootFeatureFolder;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.StartsWith("/"))
                    value = $"/{value}";
                if (!string.IsNullOrEmpty(value) && value.EndsWith("/"))
                    value = value.Substring(0, value.Length - 1);
                _rootFeatureFolder = value;
            }
        }
        public List<string> SharedViewFolders { get; set; } = new List<string>();
    }
}
