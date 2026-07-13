namespace Estapar.Garage.Api.Filters;

/// <summary>
/// Protege um endpoint com uma API Key estática (header "X-Api-Key"), em vez de JWT —
/// usado no GET /garage, chamado apenas pelo Estapar.ParkingManager.Api (serviço-a-serviço).
/// </summary>
public sealed class ApiKeyEndpointFilter(IConfiguration configuration) : IEndpointFilter
{
    private const string HeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expectedApiKey = configuration["ApiKey"];

        if (string.IsNullOrEmpty(expectedApiKey)
            || !context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedApiKey)
            || providedApiKey != expectedApiKey)
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
