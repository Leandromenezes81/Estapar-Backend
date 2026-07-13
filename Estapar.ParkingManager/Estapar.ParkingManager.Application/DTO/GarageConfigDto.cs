using System.Text.Json.Serialization;

namespace Estapar.ParkingManager.Application.DTO;

/// <summary>Shape returned by the Estapar.Garage.Api's GET /garage endpoint: every garage, with its sectors and spots.</summary>
public sealed record GarageConfigDto(
    [property: JsonPropertyName("sectors")] List<SectorConfigDto> Sectors);

public sealed record SectorConfigDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("basePrice")] decimal BasePrice,
    [property: JsonPropertyName("maxCapacity")] int MaxCapacity,
    [property: JsonPropertyName("spots")] List<SpotConfigDto> Spots);

public sealed record SpotConfigDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lng")] double Lng);
