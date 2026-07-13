using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Estapar.Garage.Api.Filters;

/// <summary>
/// Substitui, na documentação Swagger, o requirement global de Bearer pelo esquema ApiKey
/// nos endpoints marcados com <see cref="ApiKeyAuthAttribute"/> (hoje, só GET /garage).
/// </summary>
public sealed class ApiKeySecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isApiKeyProtected = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<ApiKeyAuthAttribute>()
            .Any();

        if (!isApiKeyProtected)
            return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                }] = []
            }
        ];
    }
}
