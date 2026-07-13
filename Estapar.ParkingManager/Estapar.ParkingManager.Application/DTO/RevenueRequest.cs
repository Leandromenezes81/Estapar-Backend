using System.Text.Json.Serialization;

namespace Estapar.ParkingManager.Application.DTO;

/// <summary>Corpo JSON aceito pelo endpoint GET /revenue.</summary>
public sealed record RevenueRequest(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("sector")] int Sector);
