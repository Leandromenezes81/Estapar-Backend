using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.UseCases.GarageBootstrap;
using Estapar.ParkingManager.Domain.Entities;
using Estapar.ParkingManager.Tests.Application.Fakes;
using FluentAssertions;

namespace Estapar.ParkingManager.Tests.Application;

public class LoadGarageConfigurationUseCaseTests
{
    [Fact]
    public async Task HandleAsync_SectorsWithSameNameAcrossGarages_AreKeptAsDistinctSectors()
    {
        // Regressão do bug corrigido nesta sessão: o /garage identifica cada setor pelo
        // seu Id (único por garagem), não pelo nome — o mesmo nome ("A") se repete em
        // garagens diferentes e não deve ser deduplicado.
        var garage1 = new GarageConfigDto(new List<SectorConfigDto>
        {
            new(1, "A", 10m, 20, new List<SpotConfigDto> { new(1, -23.1, -46.1) })
        });
        var garage2 = new GarageConfigDto(new List<SectorConfigDto>
        {
            new(5, "A", 10m, 20, new List<SpotConfigDto> { new(81, -23.2, -46.2) })
        });

        var client = new FakeGarageConfigClient(garage1, garage2);
        var sectors = new FakeSectorRepository();
        var spots = new FakeSpotRepository();
        var unitOfWork = new FakeUnitOfWork();

        var useCase = new LoadGarageConfigurationUseCase(client, sectors, spots, unitOfWork);
        await useCase.HandleAsync();

        sectors.Sectors.Should().HaveCount(2);
        sectors.Sectors.Select(s => s.Id).Should().BeEquivalentTo(new[] { 1, 5 });
        spots.Spots.Single(s => s.Id == 1).SectorId.Should().Be(1);
        spots.Spots.Single(s => s.Id == 81).SectorId.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_SectorAndSpotAlreadyPersisted_AreNotDuplicated()
    {
        var existingSector = Sector.Create(1, "A", 10m, 20);
        var existingSpot = Spot.Create(1, 1, -23.1, -46.1);
        var sectors = new FakeSectorRepository(existingSector);
        var spots = new FakeSpotRepository(existingSpot);
        var unitOfWork = new FakeUnitOfWork();

        var garage = new GarageConfigDto(new List<SectorConfigDto>
        {
            new(1, "A", 10m, 20, new List<SpotConfigDto> { new(1, -23.1, -46.1) })
        });
        var client = new FakeGarageConfigClient(garage);

        var useCase = new LoadGarageConfigurationUseCase(client, sectors, spots, unitOfWork);
        await useCase.HandleAsync();

        sectors.Sectors.Should().ContainSingle();
        spots.Spots.Should().ContainSingle();
    }
}
