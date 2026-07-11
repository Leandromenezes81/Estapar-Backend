using System.Text.Json.Serialization;

namespace Estapar.Application.DTOs;

/// <summary>Shape returned by the simulator's GET /garage endpoint.</summary>
public sealed record GarageConfigDto(
    [property: JsonPropertyName("garage")] List<SectorConfigDto> Garage,
    [property: JsonPropertyName("spots")] List<SpotConfigDto> Spots);

public sealed record SectorConfigDto(
    [property: JsonPropertyName("sector")] string Sector,
    [property: JsonPropertyName("basePrice")] decimal BasePrice,
    [property: JsonPropertyName("max_capacity")] int MaxCapacity);

public sealed record SpotConfigDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("sector")] string Sector,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lng")] double Lng);
