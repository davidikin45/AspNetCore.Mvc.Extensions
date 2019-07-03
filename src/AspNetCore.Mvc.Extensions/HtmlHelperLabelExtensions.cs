using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperLabelExtensions
    {
        public static IHtmlContent BootstrapLabel(this IHtmlHelper helper, string propertyName, int cololumns = 2)
        {
            string @class = "col-md-" + cololumns + " form-control-label col-form-label";
            var label = helper.Label(propertyName, null, new
            {
                @class = @class
            });

            return label;
        }
    }
}
