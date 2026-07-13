using Estapar.Garage.Api.Application.DTO;
using Estapar.Garage.Api.Auth;

namespace Estapar.Garage.Api.Endpoints;

/// <summary>Endpoints de autenticação (emissão de token JWT) da Estapar.Garage.Api.</summary>
public static class AuthEndpoints
{
    /// <summary>Mapeia o endpoint POST /auth/token.</summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", (TokenRequest request, IConfiguration configuration, JwtTokenService tokenService) =>
            {
                var clientId = configuration["Auth:ClientId"];
                var clientSecret = configuration["Auth:ClientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret)
                    || request.ClientId != clientId || request.ClientSecret != clientSecret)
                {
                    return Results.Unauthorized();
                }

                var (accessToken, expiresIn) = tokenService.GenerateToken(request.ClientId);
                return Results.Ok(new TokenResponse(accessToken, expiresIn));
            })
            .WithName("GetGarageAuthToken")
            .WithTags("Auth")
            .WithSummary("Emite um JWT a partir de client_id/client_secret configurados (sem base de usuários).")
            .AllowAnonymous()
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
