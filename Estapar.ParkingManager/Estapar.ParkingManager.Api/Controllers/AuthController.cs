using System.Net;
using Estapar.ParkingManager.Api.Auth;
using Estapar.ParkingManager.Api.DTO;
using Estapar.ParkingManager.Application.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Estapar.ParkingManager.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly JwtTokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly Response _response;

    public AuthController(JwtTokenService tokenService, IConfiguration configuration, Response response)
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _response = response;
    }

    /// <summary>Emite um JWT a partir de client_id/client_secret configurados (sem base de usuários).</summary>
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest request)
    {
        var clientId = _configuration["Auth:ClientId"];
        var clientSecret = _configuration["Auth:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret)
            || request.ClientId != clientId || request.ClientSecret != clientSecret)
        {
            _response.AddErrorMessages("client_id ou client_secret inválidos.");
            var unauthorized = await _response.GenerateResponse(HttpStatusCode.Unauthorized);
            return Unauthorized(unauthorized);
        }

        var (accessToken, expiresIn) = _tokenService.GenerateToken(request.ClientId);
        var token = new TokenResponse(accessToken, expiresIn);

        var response = await _response.GenerateResponse(HttpStatusCode.OK, "Token gerado com sucesso.", token, count: 1);
        return Ok(response);
    }
}
