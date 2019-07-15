using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Services
{
    public class JsonNavigationService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly Dictionary<string, List<NavigationItem>> _navigations = new Dictionary<string, List<NavigationItem>>();

        public JsonNavigationService(IHostingEnvironment hostingEnvironment, IOptions<JsonNavigationServiceOptions> options)
        {
            _hostingEnvironment = hostingEnvironment;

            foreach (var file in options.Value.Filenames)
            {
                _navigations.Add(file, JsonConvert.DeserializeObject<List<NavigationItem>>(File.ReadAllText(Path.Combine(_hostingEnvironment.ContentRootPath, file))));
            }
        }

        public IEnumerable<NavigationItem> GetNavigation(string filename)
        {
            return _navigations[filename].Where(n => !n.Active.HasValue || n.Active.Value);
        }
    }

    public class JsonNavigationServiceOptions
    {
        public List<string> Filenames = new List<string>() { "navigation.json" };
    }

    public class NavigationItem
    {
        public bool? Active { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string LinkText { get; set; }
    }
}
