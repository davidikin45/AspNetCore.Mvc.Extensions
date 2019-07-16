using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    public class SwaggerModelExamples : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type == typeof(Microsoft.AspNetCore.JsonPatch.Operations.Operation))
            {
                //model.Example = new { op = "add/replace/remove/copy/move/test", path = "/property", value = "value", from = "/property" };
            };
        }
    }
}
