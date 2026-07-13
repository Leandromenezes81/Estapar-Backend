namespace Estapar.Garage.Api.Application.DTO;

public sealed record CreateGarageRequest(string Name, List<CreateSectorRequest> Sectors);

public sealed record CreateSectorRequest(string Name, decimal BasePrice, int MaxCapacity, List<CreateSpotRequest> Spots);

public sealed record CreateSpotRequest(string Code, double Lat, double Lng);

/// <summary>
/// Replaces the garage's name and its whole set of sectors/spots. Existing sectors/spots
/// are soft-deleted and the ones described here are created anew (see
/// Garage.ReplaceSectorsAndSpots) — simpler and safer than reconciling children by id.
/// </summary>
public sealed record UpdateGarageRequest(string Name, List<UpdateSectorRequest> Sectors);

public sealed record UpdateSectorRequest(string Name, decimal BasePrice, int MaxCapacity, List<UpdateSpotRequest> Spots);

public sealed record UpdateSpotRequest(string Code, double Lat, double Lng);
