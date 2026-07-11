using System.Text.Json.Serialization;

namespace Estapar.Application.DTOs;

public sealed record RevenueResponse(
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp);
