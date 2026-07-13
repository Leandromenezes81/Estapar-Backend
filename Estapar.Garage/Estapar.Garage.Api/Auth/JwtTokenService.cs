using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Estapar.Garage.Api.Auth;

/// <summary>Emite JWTs assinados com a chave simétrica configurada em "Jwt:Key".</summary>
public sealed class JwtTokenService(IConfiguration configuration)
{
    /// <summary>Gera um token JWT assinado para o cliente informado, com o tempo de expiração configurado em "Jwt:ExpirationMinutes".</summary>
    public (string AccessToken, int ExpiresIn) GenerateToken(string clientId)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("A configuração 'Jwt:Key' não foi definida.");
        var expirationMinutes = jwtSection.GetValue<int?>("ExpirationMinutes") ?? 60;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, clientId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expirationMinutes * 60);
    }
}
