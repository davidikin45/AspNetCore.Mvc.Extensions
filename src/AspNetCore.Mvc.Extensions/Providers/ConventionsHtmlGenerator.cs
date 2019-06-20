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
        private readonly ConventionsHtmlGeneratorOptions _options;

        public ConventionsHtmlGenerator(IAntiforgery antiforgery, IOptions<MvcViewOptions> optionsAccessor, IModelMetadataProvider metadataProvider, IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder, ValidationHtmlAttributeProvider validationAttributeProvider, IOptions<ConventionsHtmlGeneratorOptions> options)
            : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider)
        {
            _options = options.Value;
        }

        public override TagBuilder GenerateLabel(ViewContext viewContext, ModelExplorer modelExplorer, string expression, string labelText, object htmlAttributes)
        {
            var builder = base.GenerateLabel(viewContext, modelExplorer, expression, labelText, htmlAttributes);

            if (modelExplorer.Metadata.IsRequired && _options.AddAstertix(viewContext))
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

    public class ConventionsHtmlGeneratorOptions
    {
        public Func<ViewContext, bool> AddAstertix { get; set; } = ((viewContext) => true);
    }
}
