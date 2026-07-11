using System.Text.Json.Serialization;

namespace Estapar.Application.DTOs;

public sealed record RevenueRequest(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("sector")] string Sector);
