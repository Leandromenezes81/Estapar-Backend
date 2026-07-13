using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Domain.ValueObjects;

namespace Estapar.ParkingManager.Application.UseCases.Revenue;

/// <summary>Calcula a receita total de um setor em uma data específica.</summary>
public sealed class GetRevenueUseCase
{
    private readonly IParkingSessionRepository _sessions;

    public GetRevenueUseCase(IParkingSessionRepository sessions)
    {
        _sessions = sessions;
    }

    /// <summary>Soma o valor cobrado nas sessões encerradas no setor e data informados.</summary>
    public async Task<Money> HandleAsync(RevenueQuery query, CancellationToken cancellationToken = default)
    {
        var total = await _sessions.SumRevenueAsync(query.SectorId, query.Date, cancellationToken);
        return Money.Create(total);
    }
}
