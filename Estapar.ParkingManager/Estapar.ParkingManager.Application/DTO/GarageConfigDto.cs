using System.Text.Json.Serialization;

namespace Estapar.ParkingManager.Application.DTO;

/// <summary>Formato retornado pelo endpoint GET /garage da Estapar.Garage.Api: cada garagem, com seus setores e vagas.</summary>
public sealed record GarageConfigDto(
    [property: JsonPropertyName("sectors")] List<SectorConfigDto> Sectors);

/// <summary>Representa um setor de uma garagem, com seu preço base, capacidade máxima e vagas.</summary>
public sealed record SectorConfigDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("basePrice")] decimal BasePrice,
    [property: JsonPropertyName("maxCapacity")] int MaxCapacity,
    [property: JsonPropertyName("spots")] List<SpotConfigDto> Spots);

/// <summary>Representa uma vaga física de um setor, com sua localização geográfica.</summary>
public sealed record SpotConfigDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lng")] double Lng);
