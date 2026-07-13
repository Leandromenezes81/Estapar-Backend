using System.Text;
using Estapar.ParkingManager.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Estapar.ParkingManager.Api.Registration;

/// <summary>
/// Classe estática para DI da autenticação JWT da própria Api (emissão e validação de token).
/// GET /garage (chamada ao Estapar.Garage.Api) não usa isso — é protegido por API Key, ver
/// InfrastructureDataRegistration.
/// </summary>
public static class AuthRegistration
{
    /// <summary>
    /// Método de extensão para registro da autenticação JWT no contexto de DI
    /// </summary>
    /// <param name="services">IServiceCollection Interface</param>
    /// <param name="configuration">IConfiguration Interface</param>
    /// <returns>IServiceCollection para encadeamento.</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"]
            ?? throw new InvalidOperationException("A configuração 'Jwt:Key' não foi definida.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true
                };
            });

        services.AddAuthorization();
        services.AddScoped<JwtTokenService>();

        return services;
    }
}
