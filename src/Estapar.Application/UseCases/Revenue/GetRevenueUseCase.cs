using Estapar.Application.Interfaces;
using Estapar.Domain.ValueObjects;

namespace Estapar.Application.UseCases.Revenue;

/// <summary>Computes the total revenue for a sector on a given date.</summary>
public sealed class GetRevenueUseCase
{
    private readonly IParkingSessionRepository _sessions;

    public GetRevenueUseCase(IParkingSessionRepository sessions)
    {
        _sessions = sessions;
    }

    public async Task<Money> HandleAsync(RevenueQuery query, CancellationToken cancellationToken = default)
    {
        var total = await _sessions.SumRevenueAsync(query.Sector, query.Date, cancellationToken);
        return Money.Create(total);
    }
}
