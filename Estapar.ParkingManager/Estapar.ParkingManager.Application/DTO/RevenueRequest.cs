using System.Text.Json.Serialization;

namespace Estapar.ParkingManager.Application.DTO;

public sealed record RevenueRequest(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("sector")] int Sector);
