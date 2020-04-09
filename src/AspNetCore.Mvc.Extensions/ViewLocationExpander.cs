using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private readonly string MvcImplementationFolder;

        public ViewLocationExpander(string MvcImplementationFolder = "Controllers/")
        {
            this.MvcImplementationFolder = MvcImplementationFolder;
        }

        private string[] Locations()
        {
            string[] locations = {
                "~/" + MvcImplementationFolder + "{1}/Views/{0}.cshtml",
                "~/" + MvcImplementationFolder + "Shared/Views/{0}.cshtml"
            };

            return locations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        //The view locations passed to ExpandViewLocations are:
        // /Views/{1}/{0}.cshtml
        // /Shared/{0}.cshtml
        // /Pages/{0}.cshtml

        // /Areas/{2}/Views/{1}/{0}.cshtml
        // /Areas/{2}/Shared/{0}.cshtml
        // /Areas/{2}/Pages/{0}.cshtml
        //Where {0} is the view, {1} the controller name, {2} the area name

        // Replacements
        // Views/{1} = {1}/Views
        // Shared = Shared/Views
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return viewLocations.Union(Locations());
        }
    }
}
