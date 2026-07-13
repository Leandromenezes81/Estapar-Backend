using Estapar.ParkingManager.Infrastructure.BackgroundServices.Jobs;
using Estapar.ParkingManager.Infrastructure.BackgroundServices.Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Estapar.ParkingManager.Infrastructure.BackgroundServices.Registration;

/// <summary>
/// Classe estática para Background Services - DI
/// </summary>
public static class BackgroundJobsRegistration
{
    /// <summary>
    /// Método de extensão para registro dos serviços de Background Services no contexto de DI
    /// </summary>
    /// <param name="services">IServiceCollection Interface</param>
    /// <param name="configuration">IConfiguration Interface</param>
    /// <returns>Método sem retorno.</returns>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region Quartz.NET
        services.AddQuartz(quartz =>
        {
            // Executa logo após o startup (5s) e depois re-sincroniza periodicamente (5min),
            // já que novas garagens/setores podem ser cadastrados no simulador após o boot.
            // Substitui o antigo GarageBootstrapHostedService (que só rodava uma vez, no startup).
            quartz.AddQuartzJobWithSimpleTrigger<LoadGarageConfigurationJob>(
                delay: TimeSpan.FromSeconds(5),
                repeatCount: -1,
                interval: TimeSpan.FromMinutes(5));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        #endregion

        return services;
    }
}
