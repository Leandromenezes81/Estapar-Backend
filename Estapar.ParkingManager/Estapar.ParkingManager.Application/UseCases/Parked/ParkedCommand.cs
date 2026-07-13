namespace Estapar.ParkingManager.Application.UseCases.Parked;

public sealed record ParkedCommand(int SectorId, string LicensePlate, double Lat, double Lng);
