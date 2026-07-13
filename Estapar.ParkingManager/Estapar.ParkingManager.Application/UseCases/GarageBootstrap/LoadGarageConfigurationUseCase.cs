using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.Entities;

namespace Estapar.ParkingManager.Application.UseCases.GarageBootstrap;

/// <summary>
/// Busca a configuração de garagens/vagas no endpoint GET /garage do
/// simulador e a persiste. Idempotente: setores/vagas já existentes
/// (identificados pela chave natural) são deixados intactos.
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

    /// <summary>Carrega a configuração de todas as garagens e persiste os setores e vagas ainda não cadastrados.</summary>
    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        var garages = await _garageConfigClient.GetGarageConfigurationAsync(cancellationToken);

        // Setores são identificados pelo Id atribuído pela Garage API, que é único
        // por garagem mesmo quando o nome do setor (ex.: "A") se repete entre
        // garagens diferentes — rastreia os Ids já tratados nesta execução para não
        // tentar adicionar o mesmo setor ainda não salvo duas vezes.
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
