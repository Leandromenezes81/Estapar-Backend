using Estapar.Application.Interfaces;
using Estapar.Domain.Entities;

namespace Estapar.Application.UseCases.GarageBootstrap;

/// <summary>
/// Fetches the garage/spot configuration from the simulator's GET /garage
/// endpoint and persists it. Idempotent: existing sectors/spots (matched
/// by their natural key) are left untouched.
/// </summary>
public sealed class LoadGarageConfigurationUseCase
{
    private readonly IGarageConfigClient _garageConfigClient;
    private readonly ISectorRepository _sectors;
    private readonly ISpotRepository _spots;
    private readonly IUnitOfWork _unitOfWork;

    public LoadGarageConfigurationUseCase(
        IGarageConfigClient garageConfigClient,
        ISectorRepository sectors,
        ISpotRepository spots,
        IUnitOfWork unitOfWork)
    {
        _garageConfigClient = garageConfigClient;
        _sectors = sectors;
        _spots = spots;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        var config = await _garageConfigClient.GetGarageConfigurationAsync(cancellationToken);

        foreach (var sectorDto in config.Garage)
        {
            var existing = await _sectors.GetByNameAsync(sectorDto.Sector, cancellationToken);
            if (existing is null)
            {
                var sector = Sector.Create(sectorDto.Sector, sectorDto.BasePrice, sectorDto.MaxCapacity);
                await _sectors.AddAsync(sector, cancellationToken);
            }
        }

        foreach (var spotDto in config.Spots)
        {
            var existing = await _spots.GetByIdAsync(spotDto.Id, cancellationToken);
            if (existing is null)
            {
                var spot = Spot.Create(spotDto.Id, spotDto.Sector, spotDto.Lat, spotDto.Lng);
                await _spots.AddAsync(spot, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
