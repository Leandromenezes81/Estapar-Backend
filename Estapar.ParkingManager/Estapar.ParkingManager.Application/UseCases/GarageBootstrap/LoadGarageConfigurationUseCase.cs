using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.UseCases.GarageBootstrap;

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
        var garages = await _garageConfigClient.GetGarageConfigurationAsync(cancellationToken);

        // Sectors are identified by the Id assigned by the Garage API, which is unique
        // per garage even when the sector's name (e.g. "A") repeats across garages —
        // track Ids already handled in this run so we don't try to add the same
        // not-yet-saved sector twice.
        var handledSectorIds = new HashSet<int>();

        foreach (var garageDto in garages)
        {
            foreach (var sectorDto in garageDto.Sectors)
            {
                if (handledSectorIds.Add(sectorDto.Id))
                {
                    var existingSector = await _sectors.GetByIdAsync(sectorDto.Id, cancellationToken);
                    if (existingSector is null)
                    {
                        var sector = Sector.Create(sectorDto.Id, sectorDto.Name, sectorDto.BasePrice, sectorDto.MaxCapacity);
                        await _sectors.AddAsync(sector, cancellationToken);
                    }
                }

                foreach (var spotDto in sectorDto.Spots)
                {
                    var existingSpot = await _spots.GetByIdAsync(spotDto.Id, cancellationToken);
                    if (existingSpot is null)
                    {
                        var spot = Spot.Create(spotDto.Id, sectorDto.Id, spotDto.Lat, spotDto.Lng);
                        await _spots.AddAsync(spot, cancellationToken);
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
