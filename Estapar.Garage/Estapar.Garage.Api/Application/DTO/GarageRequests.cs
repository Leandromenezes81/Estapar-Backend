namespace Estapar.Garage.Api.Application.DTO;

/// <summary>Corpo aceito pelo endpoint POST /garages para criar uma garagem com seus setores e vagas.</summary>
public sealed record CreateGarageRequest(string Name, List<CreateSectorRequest> Sectors);

/// <summary>Setor informado na criação de uma garagem.</summary>
public sealed record CreateSectorRequest(string Name, decimal BasePrice, int MaxCapacity, List<CreateSpotRequest> Spots);

/// <summary>Vaga informada na criação de um setor.</summary>
public sealed record CreateSpotRequest(string Code, double Lat, double Lng);

/// <summary>
/// Substitui o nome da garagem e todo o seu conjunto de setores/vagas. Os setores/vagas
/// existentes são removidos (soft delete) e os descritos aqui são criados novamente (ver
/// Garage.ReplaceSectorsAndSpots) — mais simples e seguro do que reconciliar filhos por Id.
/// </summary>
public sealed record UpdateGarageRequest(string Name, List<UpdateSectorRequest> Sectors);

/// <summary>Setor informado na atualização de uma garagem.</summary>
public sealed record UpdateSectorRequest(string Name, decimal BasePrice, int MaxCapacity, List<UpdateSpotRequest> Spots);

/// <summary>Vaga informada na atualização de um setor.</summary>
public sealed record UpdateSpotRequest(string Code, double Lat, double Lng);
