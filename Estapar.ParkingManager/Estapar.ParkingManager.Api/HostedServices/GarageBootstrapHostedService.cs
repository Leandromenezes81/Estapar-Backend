using Estapar.ParkingManager.Application.UseCases.GarageBootstrap;

namespace Estapar.ParkingManager.Api.HostedServices;

/// <summary>On application startup, fetches and persists the garage/spot configuration from the simulator.</summary>
public sealed class GarageBootstrapHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GarageBootstrapHostedService> _logger;

    public GarageBootstrapHostedService(IServiceProvider serviceProvider, ILogger<GarageBootstrapHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<LoadGarageConfigurationUseCase>();

        _logger.LogInformation("Buscando configuração da garagem no simulador...");
        await useCase.HandleAsync(cancellationToken);
        _logger.LogInformation("Configuração da garagem carregada.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
