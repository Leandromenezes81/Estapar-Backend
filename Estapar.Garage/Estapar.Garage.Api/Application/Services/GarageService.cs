using Estapar.Garage.Api.Application.DTO;
using Estapar.Garage.Api.Application.Interfaces;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Application.Services;

/// <summary>Use cases for the Garage aggregate: the /garage integration contract and the CRUD.</summary>
public sealed class GarageService
{
    private readonly IGarageRepository _repository;

    public GarageService(IGarageRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Builds the GET /garage payload consumed by Estapar.ParkingManager — same aggregate shape as GetByIdAsync, for every garage.</summary>
    public async Task<List<GarageResponse>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var garages = await _repository.GetAllAsync(cancellationToken);
        return garages.Select(ToResponse).ToList();
    }

    public async Task<GarageResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var garage = await _repository.GetByIdAsync(id, cancellationToken);
        return garage is null ? null : ToResponse(garage);
    }

    public async Task<GarageResponse> CreateAsync(CreateGarageRequest request, CancellationToken cancellationToken = default)
    {
        var garage = GarageEntity.Create(request.Name);

        foreach (var sectorRequest in request.Sectors)
        {
            var sector = garage.AddSector(sectorRequest.Name, sectorRequest.BasePrice, sectorRequest.MaxCapacity);
            foreach (var spotRequest in sectorRequest.Spots)
                garage.AddSpot(sector, spotRequest.Code, spotRequest.Lat, spotRequest.Lng);
        }

        await _repository.AddAsync(garage, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponse(garage);
    }

    public async Task<bool> UpdateAsync(int id, UpdateGarageRequest request, CancellationToken cancellationToken = default)
    {
        var garage = await _repository.GetByIdAsync(id, cancellationToken);
        if (garage is null) return false;

        garage.Rename(request.Name);
        garage.ReplaceSectorsAndSpots(request.Sectors.Select(s =>
            (s.Name, s.BasePrice, s.MaxCapacity, s.Spots.Select(sp => (sp.Code, sp.Lat, sp.Lng)))));

        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var garage = await _repository.GetByIdAsync(id, cancellationToken);
        if (garage is null) return false;

        garage.SoftDelete();
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static GarageResponse ToResponse(GarageEntity garage) => new(
        garage.Id,
        garage.Name,
        garage.CreatedAt,
        garage.Sectors.Select(s => new SectorResponse(
            s.Id,
            s.Name,
            s.BasePrice,
            s.MaxCapacity,
            garage.Spots.Where(sp => sp.SectorId == s.Id)
                .Select(sp => new SpotResponse(sp.Id, sp.Code, sp.Lat, sp.Lng))
                .ToList())).ToList());
}
