namespace Estapar.ParkingManager.Api.DTO;

public sealed record TokenRequest(string ClientId, string ClientSecret);

public sealed record TokenResponse(string AccessToken, int ExpiresIn, string TokenType = "Bearer");
