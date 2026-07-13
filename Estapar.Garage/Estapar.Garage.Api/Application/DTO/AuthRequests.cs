namespace Estapar.Garage.Api.Application.DTO;

/// <summary>Corpo aceito pelo endpoint POST /auth/token.</summary>
public sealed record TokenRequest(string ClientId, string ClientSecret);

/// <summary>Resposta do endpoint POST /auth/token com o token de acesso emitido.</summary>
public sealed record TokenResponse(string AccessToken, int ExpiresIn, string TokenType = "Bearer");
