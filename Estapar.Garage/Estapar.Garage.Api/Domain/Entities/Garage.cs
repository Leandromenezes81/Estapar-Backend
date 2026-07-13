namespace Estapar.Garage.Api.Domain.Entities;

/// <summary>
/// Aggregate root: a physical garage, its logical pricing/capacity divisions (Sectors)
/// and the individual parking spots (Spots) that belong to it.
/// </summary>
public sealed class Garage
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<Sector> _sectors = [];
    public IReadOnlyCollection<Sector> Sectors => _sectors.AsReadOnly();

    private readonly List<Spot> _spots = [];
    public IReadOnlyCollection<Spot> Spots => _spots.AsReadOnly();

    private Garage() { } // EF Core

    private Garage(string name)
    {
        Name = name;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Garage Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Garage name cannot be empty.", nameof(name));

        return new Garage(name);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Garage name cannot be empty.", nameof(name));

        Name = name;
    }

    public Sector AddSector(string name, decimal basePrice, int maxCapacity)
    {
        var sector = Sector.Create(this, name, basePrice, maxCapacity);
        _sectors.Add(sector);
        return sector;
    }

    public Spot AddSpot(Sector sector, string code, double lat, double lng)
    {
        if (!ReferenceEquals(sector, _sectors.Find(s => s == sector)))
            throw new InvalidOperationException("Sector does not belong to this garage.");

        var spot = Spot.Create(this, sector, code, lat, lng);
        _spots.Add(spot);
        return spot;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        foreach (var sector in _sectors) sector.SoftDelete();
        foreach (var spot in _spots) spot.SoftDelete();
    }

    /// <summary>
    /// Replaces the whole set of sectors/spots: soft-deletes everything currently
    /// attached and re-creates it from <paramref name="sectors"/>. Simpler and safer
    /// than reconciling child entities one by one for a CRUD scoped to the aggregate.
    /// </summary>
    public void ReplaceSectorsAndSpots(IEnumerable<(string Name, decimal BasePrice, int MaxCapacity, IEnumerable<(string Code, double Lat, double Lng)> Spots)> sectors)
    {
        foreach (var sector in _sectors) sector.SoftDelete();
        foreach (var spot in _spots) spot.SoftDelete();

        foreach (var sectorSpec in sectors)
        {
            var sector = AddSector(sectorSpec.Name, sectorSpec.BasePrice, sectorSpec.MaxCapacity);
            foreach (var (code, lat, lng) in sectorSpec.Spots)
                AddSpot(sector, code, lat, lng);
        }
    }
}
