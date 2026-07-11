namespace Estapar.Domain.Services;

/// <summary>
/// Dynamic pricing table applied to a sector's base price based on its
/// occupancy ratio at the moment a vehicle enters.
/// </summary>
public static class PricingPolicy
{
    private const decimal LowOccupancyThreshold = 0.25m;
    private const decimal NormalOccupancyThreshold = 0.50m;
    private const decimal HighOccupancyThreshold = 0.75m;

    private const decimal DiscountFactor = 0.90m;
    private const decimal NormalFactor = 1.00m;
    private const decimal SurchargeFactor = 1.10m;
    private const decimal HighSurchargeFactor = 1.25m;

    /// <param name="occupancyRatio">Ratio of occupied spots to sector capacity, in the range [0, 1].</param>
    public static decimal CalculateFactor(decimal occupancyRatio)
    {
        if (occupancyRatio < 0)
            throw new ArgumentOutOfRangeException(nameof(occupancyRatio), "Occupancy ratio cannot be negative.");

        return occupancyRatio switch
        {
            < LowOccupancyThreshold => DiscountFactor,
            <= NormalOccupancyThreshold => NormalFactor,
            <= HighOccupancyThreshold => SurchargeFactor,
            _ => HighSurchargeFactor
        };
    }
}
