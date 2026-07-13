namespace Estapar.Garage.Api.Domain.Entities;

/// <summary>A logical, pricing/capacity division of a garage's pool of spots.</summary>
public sealed class Sector
{
    public int Id { get; private set; }
    public int GarageId { get; private set; }
    public Garage Garage { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    public int MaxCapacity { get; private set; }
    public bool IsDeleted { get; private set; }

    private Sector() { } // EF Core

    private Sector(Garage garage, string name, decimal basePrice, int maxCapacity)
    {
        Garage = garage;
        Name = name;
        BasePrice = basePrice;
        MaxCapacity = maxCapacity;
        IsDeleted = false;
    }

    internal static Sector Create(Garage garage, string name, decimal basePrice, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sector name cannot be empty.", nameof(name));

        if (basePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice), "Base price cannot be negative.");

        if (maxCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Max capacity must be greater than zero.");

        return new Sector(garage, name, basePrice, maxCapacity);
    }

    public void SoftDelete() => IsDeleted = true;
}
