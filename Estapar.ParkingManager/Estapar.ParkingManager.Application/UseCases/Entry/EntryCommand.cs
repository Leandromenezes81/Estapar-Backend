namespace Estapar.ParkingManager.Application.UseCases.Entry;

/// <summary>Comando com os dados necessários para processar um evento ENTRY.</summary>
public sealed record EntryCommand(string LicensePlate, int SectorId, DateTime EntryTime);
