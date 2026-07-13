using Estapar.ParkingManager.Application.DTO;

namespace Estapar.ParkingManager.Application.Interfaces;

/// <summary>Reads the garage/sector/spot configuration from the Estapar.Garage.Api's GET /garage endpoint.</summary>
public interface IGarageConfigClient
{
    Task<List<GarageConfigDto>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default);
}
