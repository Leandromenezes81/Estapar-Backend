namespace Estapar.ParkingManager.Application.UseCases.Parked;

/// <summary>Comando com os dados necessários para processar um evento PARKED.</summary>
public sealed record ParkedCommand(int SectorId, string LicensePlate, double Lat, double Lng);
