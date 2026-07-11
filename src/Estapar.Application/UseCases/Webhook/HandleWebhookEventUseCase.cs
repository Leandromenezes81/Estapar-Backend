using Estapar.Application.DTOs;
using Estapar.Application.UseCases.Entry;
using Estapar.Application.UseCases.Exit;
using Estapar.Application.UseCases.Parked;

namespace Estapar.Application.UseCases.Webhook;

/// <summary>
/// Reads the flat webhook DTO in two passes: inspects <c>event_type</c>
/// first, then dispatches to the matching use case with its required
/// fields validated.
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

    public Task HandleAsync(WebhookEventDto dto, CancellationToken cancellationToken = default) =>
        dto.EventType.Trim().ToUpperInvariant() switch
        {
            "ENTRY" => HandleEntryAsync(dto, cancellationToken),
            "PARKED" => HandleParkedAsync(dto, cancellationToken),
            "EXIT" => HandleExitAsync(dto, cancellationToken),
            _ => throw new ArgumentException($"Unknown event_type '{dto.EventType}'.")
        };

    private Task HandleEntryAsync(WebhookEventDto dto, CancellationToken cancellationToken)
    {
        if (dto.EntryTime is null)
            throw new ArgumentException("entry_time is required for ENTRY events.");
        if (string.IsNullOrWhiteSpace(dto.Sector))
            throw new ArgumentException("sector is required for ENTRY events.");

        var command = new EntryCommand(dto.LicensePlate, dto.Sector, dto.EntryTime.Value);
        return _entryUseCase.HandleAsync(command, cancellationToken);
    }

    private Task HandleParkedAsync(WebhookEventDto dto, CancellationToken cancellationToken)
    {
        if (dto.Lat is null || dto.Lng is null)
            throw new ArgumentException("lat and lng are required for PARKED events.");

        var command = new ParkedCommand(dto.LicensePlate, dto.Lat.Value, dto.Lng.Value);
        return _parkedUseCase.HandleAsync(command, cancellationToken);
    }

    private Task HandleExitAsync(WebhookEventDto dto, CancellationToken cancellationToken)
    {
        if (dto.ExitTime is null)
            throw new ArgumentException("exit_time is required for EXIT events.");

        var command = new ExitCommand(dto.LicensePlate, dto.ExitTime.Value);
        return _exitUseCase.HandleAsync(command, cancellationToken);
    }
}
