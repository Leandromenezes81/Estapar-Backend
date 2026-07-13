using System.ComponentModel;

namespace Estapar.ParkingManager.Domain.Enums;

/// <summary>Provedores de execução de jobs em segundo plano suportados pela aplicação. Atualmente apenas <see cref="Quartz"/> é utilizado.</summary>
public enum BackgroundServicesProvider
{
    [Description("Worker Services")]
    WorkerServices = 1,
    [Description("Background Services")]
    BackgroundServices = 2,
    [Description("Quartz")]
    Quartz = 3,
    [Description("Hangfire")]
    Hangfire = 4
}
