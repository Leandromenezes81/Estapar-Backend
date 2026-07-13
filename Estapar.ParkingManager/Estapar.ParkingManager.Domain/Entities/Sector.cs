using Estapar.ParkingManager.Domain.Services;

namespace Estapar.ParkingManager.Domain.Entities;

/// <summary>
/// A logical division of the garage's pool of spots, identified by name
/// (e.g. "A"). Sectors are not physical areas, just pricing/capacity groups.
/// </summary>
public sealed class Sector
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    public int MaxCapacity { get; private set; }

    private Sector() { } // EF Core

    private Sector(int id, string name, decimal basePrice, int maxCapacity)
    {
        Id = id;
        Name = name;
        BasePrice = basePrice;
        MaxCapacity = maxCapacity;
    }

    public static Sector Create(int id, string name, decimal basePrice, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do setor não pode ser vazio.", nameof(name));

        if (basePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice), "O preço base não pode ser negativo.");

        if (maxCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), "A capacidade máxima deve ser maior que zero.");

        return new Sector(id, name, basePrice, maxCapacity);
    }

    public decimal OccupancyRatio(int occupiedCount) => (decimal)occupiedCount / MaxCapacity;

    public bool IsFull(int occupiedCount) => occupiedCount >= MaxCapacity;

    /// <summary>Dynamic price factor for a vehicle entering while <paramref name="occupiedCount"/> spots are taken.</summary>
    public decimal PriceFactorFor(int occupiedCount) => PricingPolicy.CalculateFactor(OccupancyRatio(occupiedCount));
}
