using Estapar.Garage.Api.Application.DTO;
using Estapar.Garage.Api.Application.Services;
using Estapar.Garage.Tests.Application.Fakes;
using FluentAssertions;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Tests.Application;

public class GarageServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsGarageWithSectorsAndSpots()
    {
        var repository = new FakeGarageRepository();
        var service = new GarageService(repository);

        var request = new CreateGarageRequest("Garagem 01", new List<CreateSectorRequest>
        {
            new("A", 10m, 20, new List<CreateSpotRequest> { new("A1", -23.1, -46.1) })
        });

        var response = await service.CreateAsync(request);

        response.Name.Should().Be("Garagem 01");
        response.Sectors.Should().ContainSingle();
        response.Sectors[0].Spots.Should().ContainSingle();
        repository.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var service = new GarageService(new FakeGarageRepository());

        var response = await service.GetByIdAsync(999);

        response.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Found_MapsSectorsAndSpots()
    {
        var garage = GarageEntity.Create("Garagem 01");
        var sector = garage.AddSector("A", 10m, 20);
        garage.AddSpot(sector, "A1", -23.1, -46.1);
        var service = new GarageService(new FakeGarageRepository(garage));

        var response = await service.GetByIdAsync(garage.Id);

        response.Should().NotBeNull();
        response!.Sectors.Should().ContainSingle(s => s.Name == "A");
        response.Sectors[0].Spots.Should().ContainSingle(sp => sp.Code == "A1");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFalse()
    {
        var service = new GarageService(new FakeGarageRepository());

        var updated = await service.UpdateAsync(999, new UpdateGarageRequest("X", new List<UpdateSectorRequest>()));

        updated.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Found_RenamesAndReplacesSectorsAndSpots()
    {
        var garage = GarageEntity.Create("Garagem 01");
        var oldSector = garage.AddSector("A", 10m, 20);
        garage.AddSpot(oldSector, "A1", -23.1, -46.1);
        var repository = new FakeGarageRepository(garage);
        var service = new GarageService(repository);

        var request = new UpdateGarageRequest("Garagem 01 renomeada", new List<UpdateSectorRequest>
        {
            new("B", 15m, 10, new List<UpdateSpotRequest> { new("B1", -23.2, -46.2) })
        });

        var updated = await service.UpdateAsync(garage.Id, request);

        updated.Should().BeTrue();
        garage.Name.Should().Be("Garagem 01 renomeada");
        garage.Sectors.Should().Contain(s => s.Name == "B" && !s.IsDeleted);
        garage.Sectors.Should().Contain(s => s.Name == "A" && s.IsDeleted);
        repository.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task SoftDeleteAsync_NotFound_ReturnsFalse()
    {
        var service = new GarageService(new FakeGarageRepository());

        var deleted = await service.SoftDeleteAsync(999);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_Found_MarksGarageAndChildrenDeleted()
    {
        var garage = GarageEntity.Create("Garagem 01");
        var sector = garage.AddSector("A", 10m, 20);
        garage.AddSpot(sector, "A1", -23.1, -46.1);
        var repository = new FakeGarageRepository(garage);
        var service = new GarageService(repository);

        var deleted = await service.SoftDeleteAsync(garage.Id);

        deleted.Should().BeTrue();
        garage.IsDeleted.Should().BeTrue();
        garage.Sectors.Should().OnlyContain(s => s.IsDeleted);
        garage.Spots.Should().OnlyContain(sp => sp.IsDeleted);
        repository.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetGarageConfigurationAsync_ReturnsAllGaragesMapped()
    {
        var garage1 = GarageEntity.Create("Garagem 01");
        garage1.AddSpot(garage1.AddSector("A", 10m, 20), "A1", -23.1, -46.1);
        var garage2 = GarageEntity.Create("Garagem 02");
        garage2.AddSpot(garage2.AddSector("A", 10m, 20), "A1", -23.2, -46.2);

        var service = new GarageService(new FakeGarageRepository(garage1, garage2));

        var result = await service.GetGarageConfigurationAsync();

        result.Should().HaveCount(2);
    }
}
