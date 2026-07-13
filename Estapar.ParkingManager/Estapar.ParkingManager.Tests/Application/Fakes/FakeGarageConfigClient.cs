using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.Interfaces;

namespace Estapar.ParkingManager.Tests.Application.Fakes;

/// <summary>Stub de IGarageConfigClient que retorna um payload fixo de GET /garage em vez de chamar a Estapar.Garage.Api via HTTP.</summary>
public sealed class FakeGarageConfigClient : IGarageConfigClient
{
    private readonly List<GarageConfigDto> _garages;

    public FakeGarageConfigClient(params GarageConfigDto[] garages) => _garages = new List<GarageConfigDto>(garages);

    public Task<List<GarageConfigDto>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_garages);
}
