namespace Estapar.Application.UseCases.Entry;

public sealed record EntryCommand(string LicensePlate, string Sector, DateTime EntryTime);
