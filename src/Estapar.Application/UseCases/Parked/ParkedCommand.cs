namespace Estapar.Application.UseCases.Parked;

public sealed record ParkedCommand(string LicensePlate, double Lat, double Lng);
