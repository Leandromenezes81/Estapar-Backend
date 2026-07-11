namespace Estapar.Domain.Entities;

/// <summary>A single physical parking spot belonging to a sector.</summary>
public sealed class Spot
{
    public int Id { get; private set; }
    public string SectorName { get; private set; } = null!;
    public double Lat { get; private set; }
    public double Lng { get; private set; }
    public SpotStatus Status { get; private set; } = SpotStatus.Available;

    private Spot() { } // EF Core

    private Spot(int id, string sectorName, double lat, double lng)
    {
        Id = id;
        SectorName = sectorName;
        Lat = lat;
        Lng = lng;
        Status = SpotStatus.Available;
    }

    public static Spot Create(int id, string sectorName, double lat, double lng)
    {
        if (string.IsNullOrWhiteSpace(sectorName))
            throw new ArgumentException("Sector name cannot be empty.", nameof(sectorName));

        return new Spot(id, sectorName, lat, lng);
    }

    public bool IsAvailable => Status == SpotStatus.Available;

    public void Occupy()
    {
        if (Status == SpotStatus.Occupied)
            throw new InvalidOperationException($"Spot {Id} is already occupied.");

        Status = SpotStatus.Occupied;
    }

    public void Release()
    {
        Status = SpotStatus.Available;
    }
}
