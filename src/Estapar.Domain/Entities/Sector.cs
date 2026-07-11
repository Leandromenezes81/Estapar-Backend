using Estapar.Domain.Services;

namespace Estapar.Domain.Entities;

/// <summary>
/// A logical division of the garage's pool of spots, identified by name
/// (e.g. "A"). Sectors are not physical areas, just pricing/capacity groups.
/// </summary>
public sealed class Sector
{
    public string Name { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    public int MaxCapacity { get; private set; }

    private Sector() { } // EF Core

    private Sector(string name, decimal basePrice, int maxCapacity)
    {
        Name = name;
        BasePrice = basePrice;
        MaxCapacity = maxCapacity;
    }

    public static Sector Create(string name, decimal basePrice, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sector name cannot be empty.", nameof(name));

        if (basePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice), "Base price cannot be negative.");

        if (maxCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Max capacity must be greater than zero.");

        return new Sector(name, basePrice, maxCapacity);
    }

    public decimal OccupancyRatio(int occupiedCount) => (decimal)occupiedCount / MaxCapacity;

    public bool IsFull(int occupiedCount) => occupiedCount >= MaxCapacity;

    /// <summary>Dynamic price factor for a vehicle entering while <paramref name="occupiedCount"/> spots are taken.</summary>
    public decimal PriceFactorFor(int occupiedCount) => PricingPolicy.CalculateFactor(OccupancyRatio(occupiedCount));
}
