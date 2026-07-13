using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.UseCases.Entry;
using Estapar.ParkingManager.Application.UseCases.Exit;
using Estapar.ParkingManager.Application.UseCases.Parked;
using Estapar.ParkingManager.Domain.Enums;

namespace Estapar.ParkingManager.Application.UseCases.Webhook;

/// <summary>
/// Lê o DTO plano do webhook em duas etapas: primeiro inspeciona
/// <c>event_type</c> e depois despacha para o caso de uso correspondente,
/// validando os campos obrigatórios de cada tipo de evento.
/// </summary>
public sealed class HandleWebhookEventUseCase
{
    private readonly HandleEntryUseCase _entryUseCase;
    private readonly HandleParkedUseCase _parkedUseCase;
    private readonly HandleExitUseCase _exitUseCase;

    public HandleWebhookEventUseCase(
        HandleEntryUseCase entryUseCase,
        HandleParkedUseCase parkedUseCase,
        HandleExitUseCase exitUseCase)
    {
        _entryUseCase = entryUseCase;
        _parkedUseCase = parkedUseCase;
        _exitUseCase = exitUseCase;
    }

    /// <summary>Despacha o evento recebido para o caso de uso correspondente ao seu <see cref="EventType"/>.</summary>
    public Task HandleAsync(WebhookEventDto dto, CancellationToken cancellationToken = default) =>
        dto.EventType switch
        {
            EventType.ENTRY => HandleEntryAsync(dto, cancellationToken),
            EventType.PARKED => HandleParkedAsync(dto, cancellationToken),
            EventType.EXIT => HandleExitAsync(dto, cancellationToken),
            _ => throw new ArgumentException($"event_type '{dto.EventType}' desconhecido.")
        };

    private Task HandleEntryAsync(WebhookEventDto dto, CancellationToken cancellationToken)
    {
        if (dto.EntryTime is null)
            throw new ArgumentException("entry_time é obrigatório para eventos ENTRY.");
        if (dto.SectorId is null)
            throw new ArgumentException("sector é obrigatório para eventos ENTRY.");

        var command = new EntryCommand(dto.LicensePlate, dto.SectorId.Value, dto.EntryTime.Value);
        return _entryUseCase.HandleAsync(command, cancellationToken);
    }

    private Task HandleParkedAsync(WebhookEventDto dto, CancellationToken cancellationToken)
    {
        if (dto.SectorId is null || dto.Lat is null || dto.Lng is null)
            throw new ArgumentException("sector, lat e lng são obrigatórios para eventos PARKED.");

        var command = new ParkedCommand(dto.SectorId.Value, dto.LicensePlate, dto.Lat.Value, dto.Lng.Value);
        return _parkedUseCase.HandleAsync(command, cancellationToken);
    }

    private Task HandleExitAsync(WebhookEventDto dto, CancellationToken cancellationToken)
    {
        if (dto.ExitTime is null)
            throw new ArgumentException("exit_time é obrigatório para eventos EXIT.");

        var command = new ExitCommand(dto.LicensePlate, dto.ExitTime.Value);
        return _exitUseCase.HandleAsync(command, cancellationToken);
    }
}
