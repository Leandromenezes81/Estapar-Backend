namespace Estapar.Garage.Api.Infrastructure.Persistence.Seed;

/// <summary>
/// Dados de seed determinísticos usados pelas chamadas HasData das configurações de entidade:
/// 5 garagens, cada uma com 4 setores (nomeados A-D) de 20 vagas cada (nomeadas "&lt;Setor&gt;&lt;n&gt;",
/// ex.: "A1"-"A20"). Os valores devem permanecer fixos assim que uma migration for publicada —
/// o HasData compara por chave primária.
/// </summary>
public static class GarageSeedData
{
    public const int GarageCount = 5;
    public const int SectorsPerGarage = 4;
    public const int SpotsPerSector = 20;

    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Gera os dados de seed das garagens.</summary>
    public static IEnumerable<object> Garages() =>
        Enumerable.Range(1, GarageCount).Select(garageId => new
        {
            Id = garageId,
            Name = $"Garagem {garageId:00}",
            IsDeleted = false,
            CreatedAt = SeedCreatedAt
        });

    /// <summary>Gera os dados de seed dos setores, distribuídos igualmente entre as garagens.</summary>
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

    /// <summary>Gera os dados de seed das vagas, distribuídas igualmente entre os setores de cada garagem.</summary>
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

    /// <summary>Converte o índice do setor em seu nome de uma letra (0 → "A", 1 → "B", ...).</summary>
    private static string SectorName(int sectorIndex) => ((char)('A' + sectorIndex)).ToString();
}
