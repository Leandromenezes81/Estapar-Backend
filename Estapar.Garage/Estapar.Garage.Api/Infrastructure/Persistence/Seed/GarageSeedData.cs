namespace Estapar.Garage.Api.Infrastructure.Persistence.Seed;

/// <summary>
/// Deterministic seed data used by the entity configurations' HasData calls: 5 garages,
/// each with 4 sectors (named A-D) of 20 spots each (named "&lt;Sector&gt;&lt;n&gt;", e.g. "A1"-"A20").
/// Values must stay fixed once a migration has shipped — HasData diffs by primary key.
/// </summary>
public static class GarageSeedData
{
    public const int GarageCount = 5;
    public const int SectorsPerGarage = 4;
    public const int SpotsPerSector = 20;

    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static IEnumerable<object> Garages() =>
        Enumerable.Range(1, GarageCount).Select(garageId => new
        {
            Id = garageId,
            Name = $"Garagem {garageId:00}",
            IsDeleted = false,
            CreatedAt = SeedCreatedAt
        });

    public static IEnumerable<object> Sectors()
    {
        var sectorId = 1;

        for (var garageId = 1; garageId <= GarageCount; garageId++)
        {
            for (var s = 0; s < SectorsPerGarage; s++)
            {
                yield return new
                {
                    Id = sectorId++,
                    GarageId = garageId,
                    Name = SectorName(s),
                    BasePrice = 10m + s * 2.5m,
                    MaxCapacity = SpotsPerSector,
                    IsDeleted = false
                };
            }
        }
    }

    public static IEnumerable<object> Spots()
    {
        var spotId = 1;
        var sectorId = 1;

        for (var garageId = 1; garageId <= GarageCount; garageId++)
        {
            var baseLat = -23.55 - (garageId - 1) * 0.02;
            var baseLng = -46.63 + (garageId - 1) * 0.02;

            for (var s = 0; s < SectorsPerGarage; s++)
            {
                for (var n = 1; n <= SpotsPerSector; n++)
                {
                    yield return new
                    {
                        Id = spotId++,
                        GarageId = garageId,
                        SectorId = sectorId,
                        Code = $"{SectorName(s)}{n}",
                        Lat = baseLat + s * 0.001 + n * 0.0001,
                        Lng = baseLng + s * 0.001 + n * 0.0001,
                        IsDeleted = false
                    };
                }

                sectorId++;
            }
        }
    }

    private static string SectorName(int sectorIndex) => ((char)('A' + sectorIndex)).ToString();
}
