using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Infrastructure.Data.External;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Estapar.ParkingManager.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Estapar.ParkingManager.Infrastructure.Data.Registration;

/// <summary>
/// Classe estática para DI da camada Infrastructure.Data (EF Core, repositórios, cliente HTTP externo)
/// </summary>
public static class InfrastructureDataRegistration
{
    /// <summary>
    /// Método de extensão para registro dos serviços de Infrastructure.Data no contexto de DI
    /// </summary>
    /// <param name="services">IServiceCollection Interface</param>
    /// <param name="configuration">IConfiguration Interface</param>
    /// <returns>IServiceCollection para encadeamento.</returns>
    public static IServiceCollection AddInfrastructureData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<ISpotRepository, SpotRepository>();
        services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Estapar.Garage.Api HTTP client — source of the GET /garage configuration.
        // GET /garage é protegido por API Key (não JWT) do lado do Garage.Api — o header
        // é fixo por cliente HTTP, sem round-trip de token.
        services.AddHttpClient<IGarageConfigClient, GarageConfigHttpClient>(client =>
        {
            var baseUrl = configuration["GarageApi:BaseUrl"]
                ?? throw new InvalidOperationException("A configuração 'GarageApi:BaseUrl' não foi definida.");
            client.BaseAddress = new Uri(baseUrl);

            var apiKey = configuration["GarageApi:ApiKey"]
                ?? throw new InvalidOperationException("A configuração 'GarageApi:ApiKey' não foi definida.");
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        });

        return services;
    }
}
