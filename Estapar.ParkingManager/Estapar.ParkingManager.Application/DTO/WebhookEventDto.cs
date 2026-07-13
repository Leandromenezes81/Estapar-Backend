using System.Text.Json.Serialization;
using Estapar.ParkingManager.Domain.Enums;

namespace Estapar.ParkingManager.Application.DTO;

/// <summary>
/// Representação plana dos três possíveis payloads de /webhook (ENTRY,
/// PARKED, EXIT), discriminados por <see cref="EventType"/>. Campos não
/// relevantes para um determinado tipo de evento ficam nulos.
///
/// Premissa: ENTRY carrega um campo "sector" (agora contendo o Id do setor
/// em vez do seu nome) mesmo não estando presente no exemplo de ENTRY da
/// especificação de teste, já que o fator de preço dinâmico precisa ser
/// travado por setor no momento da entrada.
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
