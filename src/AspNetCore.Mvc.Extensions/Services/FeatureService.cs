using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace AspNetCore.Mvc.Extensions.Services
{
    public class FeatureService
    {
        private IWebHostEnvironment _hostingEnvironment;

        private Dictionary<string, bool> featureStates = new Dictionary<string, bool>();

        public FeatureService(IWebHostEnvironment environment)
        {
            this._hostingEnvironment = environment;
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, "features.json");

            this.featureStates =
                JsonConvert.DeserializeObject<Dictionary<string, bool>>
                (File.ReadAllText(path));
        }

        public bool IsFeatureActive(string featureName)
        {
            return featureStates[featureName];
        }
    }
}
