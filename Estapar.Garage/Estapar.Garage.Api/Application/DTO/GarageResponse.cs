namespace Estapar.Garage.Api.Application.DTO;

/// <summary>Representação de uma garagem com seus setores e vagas, retornada pelos endpoints de consulta.</summary>
public sealed record GarageResponse(int Id, string Name, DateTime CreatedAt, List<SectorResponse> Sectors);

/// <summary>Representação de um setor com suas vagas.</summary>
public sealed record SectorResponse(int Id, string Name, decimal BasePrice, int MaxCapacity, List<SpotResponse> Spots);

/// <summary>Representação de uma vaga física.</summary>
public sealed record SpotResponse(int Id, string Code, double Lat, double Lng);
