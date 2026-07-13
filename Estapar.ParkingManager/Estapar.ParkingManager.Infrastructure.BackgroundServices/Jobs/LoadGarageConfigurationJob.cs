using Estapar.ParkingManager.Application.UseCases.GarageBootstrap;
using Estapar.ParkingManager.Domain.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Estapar.ParkingManager.Infrastructure.BackgroundServices.Jobs;

/// <summary>Job do Quartz que sincroniza periodicamente a configuração de garagens/setores/vagas a partir do simulador (GET /garage).</summary>
[DisallowConcurrentExecution]
[JobAttribute(jobGroup: typeof(Domain.Entities.Sector), description: "Configuração da garagem no Simulador")]
public class LoadGarageConfigurationJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoadGarageConfigurationJob> _logger;

    public LoadGarageConfigurationJob(IServiceProvider serviceProvider, ILogger<LoadGarageConfigurationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>Executa o caso de uso de carga da configuração da garagem dentro de um escopo de DI próprio.</summary>
    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<LoadGarageConfigurationUseCase>();

        _logger.LogInformation("Buscando configuração da garagem no simulador...");
        await useCase.HandleAsync();
        _logger.LogInformation("Configuração da garagem carregada.");
    }
}
