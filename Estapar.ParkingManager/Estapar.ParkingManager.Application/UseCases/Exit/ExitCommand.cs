namespace Estapar.ParkingManager.Application.UseCases.Exit;

public sealed record ExitCommand(string LicensePlate, DateTime ExitTime);
