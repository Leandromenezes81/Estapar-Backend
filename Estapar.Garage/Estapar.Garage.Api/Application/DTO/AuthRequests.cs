namespace Estapar.Garage.Api.Application.DTO;

public sealed record TokenRequest(string ClientId, string ClientSecret);

public sealed record TokenResponse(string AccessToken, int ExpiresIn, string TokenType = "Bearer");
