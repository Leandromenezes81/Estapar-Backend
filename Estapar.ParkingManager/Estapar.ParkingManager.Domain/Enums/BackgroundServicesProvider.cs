using System.ComponentModel;

namespace Estapar.ParkingManager.Domain.Enums;

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
