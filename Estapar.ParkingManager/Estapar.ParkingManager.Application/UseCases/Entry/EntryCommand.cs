namespace Estapar.ParkingManager.Application.UseCases.Entry;

public sealed record EntryCommand(string LicensePlate, int SectorId, DateTime EntryTime);
