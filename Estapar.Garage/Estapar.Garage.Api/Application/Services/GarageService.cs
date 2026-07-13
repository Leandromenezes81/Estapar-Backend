using Estapar.Garage.Api.Application.DTO;
using Estapar.Garage.Api.Application.Interfaces;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Api.Application.Services;

/// <summary>Casos de uso do agregado Garage: o contrato de integração /garage e o CRUD.</summary>
public sealed class GarageService
{
    private readonly IGarageRepository _repository;

    public GarageService(IGarageRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Monta o payload do GET /garage consumido pelo Estapar.ParkingManager — mesmo formato de agregado do GetByIdAsync, para cada garagem.</summary>
    public async Task<List<GarageResponse>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var garages = await _repository.GetAllAsync(cancellationToken);
        return garages.Select(ToResponse).ToList();
    }

    /// <summary>Busca uma garagem pelo Id, retornando null se não existir.</summary>
    public async Task<GarageResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var garage = await _repository.GetByIdAsync(id, cancellationToken);
        return garage is null ? null : ToResponse(garage);
    }

    /// <summary>Cria uma nova garagem com os setores e vagas informados.</summary>
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

    /// <summary>Atualiza o nome e substitui todo o conjunto de setores/vagas de uma garagem, retornando false se ela não existir.</summary>
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

    /// <summary>Remove (soft delete) uma garagem e seus setores/vagas, retornando false se ela não existir.</summary>
    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var garage = await _repository.GetByIdAsync(id, cancellationToken);
        if (garage is null) return false;

        garage.SoftDelete();
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Converte a entidade de domínio <see cref="GarageEntity"/> no DTO de resposta agregado, com seus setores e vagas.</summary>
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
