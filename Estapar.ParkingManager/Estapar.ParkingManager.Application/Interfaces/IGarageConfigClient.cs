using Estapar.ParkingManager.Application.DTO;

namespace Estapar.ParkingManager.Application.Interfaces;

/// <summary>Lê a configuração de garagens/setores/vagas a partir do endpoint GET /garage da Estapar.Garage.Api.</summary>
public interface IGarageConfigClient
{
    /// <summary>Obtém a configuração completa de todas as garagens.</summary>
    Task<List<GarageConfigDto>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default);
}
