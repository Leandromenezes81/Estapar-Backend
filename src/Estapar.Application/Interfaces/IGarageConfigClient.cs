using Estapar.Application.DTOs;

namespace Estapar.Application.Interfaces;

/// <summary>Reads the garage/spot configuration from the simulator's GET /garage endpoint.</summary>
public interface IGarageConfigClient
{
    Task<GarageConfigDto> GetGarageConfigurationAsync(CancellationToken cancellationToken = default);
}
