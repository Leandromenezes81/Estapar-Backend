using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.UseCases.Entry;
using Estapar.ParkingManager.Application.UseCases.Exit;
using Estapar.ParkingManager.Application.UseCases.GarageBootstrap;
using Estapar.ParkingManager.Application.UseCases.Parked;
using Estapar.ParkingManager.Application.UseCases.Revenue;
using Estapar.ParkingManager.Application.UseCases.Webhook;
using Microsoft.Extensions.DependencyInjection;

namespace Estapar.ParkingManager.Application.Registration;

/// <summary>
/// Classe estática para DI da camada Application (casos de uso, DTOs)
/// </summary>
public static class ApplicationRegistration
{
    /// <summary>
    /// Método de extensão para registro dos serviços de Application no contexto de DI
    /// </summary>
    /// <param name="services">IServiceCollection Interface</param>
    /// <returns>IServiceCollection para encadeamento.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<HandleEntryUseCase>();
        services.AddScoped<HandleParkedUseCase>();
        services.AddScoped<HandleExitUseCase>();
        services.AddScoped<HandleWebhookEventUseCase>();
        services.AddScoped<GetRevenueUseCase>();
        services.AddScoped<LoadGarageConfigurationUseCase>();
        services.AddScoped<Response>();

        return services;
    }
}
