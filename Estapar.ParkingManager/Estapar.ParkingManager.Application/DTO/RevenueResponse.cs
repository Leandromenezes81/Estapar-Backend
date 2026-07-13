using System.Text.Json.Serialization;

namespace Estapar.ParkingManager.Application.DTO;

public sealed record RevenueResponse(
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp);
