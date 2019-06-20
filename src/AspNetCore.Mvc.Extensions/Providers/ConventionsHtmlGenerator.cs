using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Encodings.Web;

namespace AspNetCore.Mvc.Extensions.Providers
{
    public class ConventionsHtmlGenerator : DefaultHtmlGenerator, IHtmlGenerator
    {
        private readonly Func<ViewContext, bool> _addAstertix;
        public ConventionsHtmlGenerator(IAntiforgery antiforgery, IOptions<MvcViewOptions> optionsAccessor, IModelMetadataProvider metadataProvider, IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder, ValidationHtmlAttributeProvider validationAttributeProvider, Func<ViewContext, bool> addAstertix)
            : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider)
        {
            _addAstertix = addAstertix;
        }

        public override TagBuilder GenerateLabel(ViewContext viewContext, ModelExplorer modelExplorer, string expression, string labelText, object htmlAttributes)
        {
            var builder = base.GenerateLabel(viewContext, modelExplorer, expression, labelText, htmlAttributes);

            if (modelExplorer.Metadata.IsRequired && _addAstertix(viewContext))
            {
                var newLabelText = Render(builder.InnerHtml) + " *";
                builder.InnerHtml.SetContent(newLabelText);
            }

            return builder;
        }

        public string Render(IHtmlContent htmlContent)
        {
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
