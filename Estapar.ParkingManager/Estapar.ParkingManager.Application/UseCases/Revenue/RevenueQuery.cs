namespace Estapar.ParkingManager.Application.UseCases.Revenue;

/// <summary>Consulta de receita de um setor em uma data específica.</summary>
public sealed record RevenueQuery(int SectorId, DateOnly Date);
