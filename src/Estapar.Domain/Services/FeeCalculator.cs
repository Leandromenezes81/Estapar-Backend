using Estapar.Domain.ValueObjects;

namespace Estapar.Domain.Services;

/// <summary>
/// Calculates the fee charged for a parking session: the first 30 minutes
/// are free, and every started hour after that is charged at the sector's
/// base price adjusted by the dynamic price factor locked in at entry.
/// </summary>
public static class FeeCalculator
{
    private static readonly TimeSpan FreeThreshold = TimeSpan.FromMinutes(30);

    public static Money Calculate(DateTime entryTime, DateTime exitTime, decimal basePrice, decimal priceFactor)
    {
        if (exitTime < entryTime)
            throw new ArgumentOutOfRangeException(nameof(exitTime), "Exit time cannot be before entry time.");

        var duration = exitTime - entryTime;
        if (duration <= FreeThreshold)
            return Money.Zero();

        var billableHours = (int)Math.Ceiling(duration.TotalHours);
        var amount = billableHours * basePrice * priceFactor;

        return Money.Create(amount);
    }
}
