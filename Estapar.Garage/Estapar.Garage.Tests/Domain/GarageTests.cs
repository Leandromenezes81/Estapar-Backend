using FluentAssertions;
using GarageEntity = Estapar.Garage.Api.Domain.Entities.Garage;

namespace Estapar.Garage.Tests.Domain;

public class GarageTests
{
    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        var act = () => GarageEntity.Create("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ValidName_IsNotDeleted()
    {
        var garage = GarageEntity.Create("Garagem 01");

        garage.Name.Should().Be("Garagem 01");
        garage.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void AddSector_EmptyName_ThrowsArgumentException()
    {
        var garage = GarageEntity.Create("Garagem 01");

        var act = () => garage.AddSector("", 10m, 20);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddSector_NegativeBasePrice_ThrowsArgumentOutOfRangeException()
    {
        var garage = GarageEntity.Create("Garagem 01");

        var act = () => garage.AddSector("A", -1m, 20);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddSector_ZeroMaxCapacity_ThrowsArgumentOutOfRangeException()
    {
        var garage = GarageEntity.Create("Garagem 01");

        var act = () => garage.AddSector("A", 10m, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddSpot_EmptyCode_ThrowsArgumentException()
    {
        var garage = GarageEntity.Create("Garagem 01");
        var sector = garage.AddSector("A", 10m, 20);

        var act = () => garage.AddSpot(sector, "", -23.1, -46.1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddSpot_SectorFromAnotherGarage_ThrowsInvalidOperationException()
    {
        var garage1 = GarageEntity.Create("Garagem 01");
        var garage2 = GarageEntity.Create("Garagem 02");
        var sectorFromGarage2 = garage2.AddSector("A", 10m, 20);

        var act = () => garage1.AddSpot(sectorFromGarage2, "A1", -23.1, -46.1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SoftDelete_CascadesToSectorsAndSpots()
    {
        var garage = GarageEntity.Create("Garagem 01");
        var sector = garage.AddSector("A", 10m, 20);
        garage.AddSpot(sector, "A1", -23.1, -46.1);

        garage.SoftDelete();

        garage.IsDeleted.Should().BeTrue();
        garage.Sectors.Should().OnlyContain(s => s.IsDeleted);
        garage.Spots.Should().OnlyContain(sp => sp.IsDeleted);
    }

    [Fact]
    public void ReplaceSectorsAndSpots_SoftDeletesOldAndCreatesNew()
    {
        var garage = GarageEntity.Create("Garagem 01");
        var oldSector = garage.AddSector("A", 10m, 20);
        garage.AddSpot(oldSector, "A1", -23.1, -46.1);

        var newSectors = new List<(string Name, decimal BasePrice, int MaxCapacity, IEnumerable<(string Code, double Lat, double Lng)> Spots)>
        {
            ("B", 15m, 10, new List<(string Code, double Lat, double Lng)> { ("B1", -23.2, -46.2) })
        };

        garage.ReplaceSectorsAndSpots(newSectors);

        garage.Sectors.Should().Contain(s => s.Name == "A" && s.IsDeleted);
        garage.Sectors.Should().Contain(s => s.Name == "B" && !s.IsDeleted);
        garage.Spots.Should().Contain(sp => sp.Code == "A1" && sp.IsDeleted);
        garage.Spots.Should().Contain(sp => sp.Code == "B1" && !sp.IsDeleted);
    }
}
