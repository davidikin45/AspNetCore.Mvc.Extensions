using Microsoft.AspNetCore.JsonPatch;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    //http://jsonpatch.com/
    public class SwaggerJsonPatchDocumentExample : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type == typeof(JsonPatchDocument))
            {
                model.Example = new OpenApiArray() {
                        new OpenApiObject(){
                            ["op"] = new OpenApiString("add"),
                            ["path"] = new OpenApiString("/propertyArray/-"),
                            ["value"] = new OpenApiObject(){
                                ["property"] = new OpenApiString("value")
                            },
                        },
                        new OpenApiObject(){
                            ["op"] = new OpenApiString("remove"),
                            ["path"] = new OpenApiString("/propertyArray/0")
                        },
                        new OpenApiObject(){
                            ["op"] = new OpenApiString("replace"),
                            ["path"] = new OpenApiString("/property"),
                            ["value"] = new OpenApiString("value"),
                        },
                        new OpenApiObject(){
                            ["op"] = new OpenApiString("copy"),
                            ["from"] = new OpenApiString("/property1"),
                            ["path"] = new OpenApiString("/property2")
                        },
                        new OpenApiObject(){
                            ["op"] = new OpenApiString("move"),
                            ["from"] = new OpenApiString("/property1"),
                            ["path"] = new OpenApiString("/property2"),
                        },
                        new OpenApiObject(){
                            ["op"] = new OpenApiString("test"),
                            ["path"] = new OpenApiString("/property"),
                            ["value"] = new OpenApiString("value")
                        }
                    };
            };
        }
    }
}
