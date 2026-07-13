namespace Estapar.ParkingManager.Application.UseCases.Exit;

/// <summary>Comando com os dados necessários para processar um evento EXIT.</summary>
public sealed record ExitCommand(string LicensePlate, DateTime ExitTime);
