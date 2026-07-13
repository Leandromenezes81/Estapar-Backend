namespace Estapar.Garage.Api.Application.DTO;

public sealed record GarageResponse(int Id, string Name, DateTime CreatedAt, List<SectorResponse> Sectors);

public sealed record SectorResponse(int Id, string Name, decimal BasePrice, int MaxCapacity, List<SpotResponse> Spots);

public sealed record SpotResponse(int Id, string Code, double Lat, double Lng);
