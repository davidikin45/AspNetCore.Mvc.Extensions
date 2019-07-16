using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.VariableResourceRepresentation
{
    public class VariableResourceRepresentationOptions
    {
        public bool ReturnHttpNotAcceptable { get; set; } = true;
        public bool RespectBrowserAcceptHeader { get; set; } = false;

        public List<string> JsonInputMediaTypes = new List<string>();
        public List<string> JsonOutputMediaTypes = new List<string>();

        public bool RemoveTextJsonTextOutputFormatter { get; set; } = true;

        public Dictionary<string, string> FormatterMappings = new Dictionary<string, string>() { {"xml", "application/xml" } };
    }
}
