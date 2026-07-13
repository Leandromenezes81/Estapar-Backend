using System.Text.Json.Serialization;
using Estapar.ParkingManager.Domain.Enums;

namespace Estapar.ParkingManager.Application.DTO;

/// <summary>
/// Flat representation of the three possible /webhook payloads (ENTRY,
/// PARKED, EXIT), discriminated by <see cref="EventType"/>. Fields not
/// relevant to a given event type are left null.
///
/// Assumption: ENTRY carries a "sector" field (now holding the sector's Id
/// rather than its name) even though it is not shown in the ENTRY example
/// of the test spec, since the dynamic price factor must be locked in
/// per-sector at entry time.
/// </summary>
public sealed class WebhookEventDto
{
    [JsonPropertyName("event_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EventType EventType { get; init; }

    [JsonPropertyName("license_plate")]
    public string LicensePlate { get; init; } = string.Empty;

    // ENTRY
    [JsonPropertyName("entry_time")]
    public DateTime? EntryTime { get; init; }

    [JsonPropertyName("sector")]
    public int? SectorId { get; init; }

    // PARKED
    [JsonPropertyName("lat")]
    public double? Lat { get; init; }

    [JsonPropertyName("lng")]
    public double? Lng { get; init; }

    // EXIT
    [JsonPropertyName("exit_time")]
    public DateTime? ExitTime { get; init; }
}
