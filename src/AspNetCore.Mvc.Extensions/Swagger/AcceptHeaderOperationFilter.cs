using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Mvc.Extensions.Swagger
{
    //Need to also add 
    //[ApiExplorerSettings(IgnoreApi = true)]
    //[Produces("application/vnd.app.bookwithconcatenatedauthorname+json")]
    //[AcceptHeaderMatchesMediaType("application/vnd.app.bookforcreationwithamountofpages+json")]
    public abstract class AcceptHeaderOperationFilter<TSchemaType> : IOperationFilter
    {
        private readonly string _operationId;
        private readonly string _acceptHeader;

        public AcceptHeaderOperationFilter(string operationId, string acceptHeader)
        {
            _operationId = operationId;
            _acceptHeader = acceptHeader;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if(operation.OperationId != _operationId)
            {
                return;
            }

            operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add(_acceptHeader, new OpenApiMediaType() {
                Schema = context.SchemaGenerator.GenerateSchema(typeof(TSchemaType), context.SchemaRepository)
            });
        }
    }
}
