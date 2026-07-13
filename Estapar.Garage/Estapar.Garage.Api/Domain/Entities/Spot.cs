namespace Estapar.Garage.Api.Domain.Entities;

/// <summary>A single physical parking spot, belonging to both a garage and one of its sectors.</summary>
public sealed class Spot
{
    public int Id { get; private set; }
    public int GarageId { get; private set; }
    public Garage Garage { get; private set; } = null!;
    public int SectorId { get; private set; }
    public Sector Sector { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public double Lat { get; private set; }
    public double Lng { get; private set; }
    public bool IsDeleted { get; private set; }

    private Spot() { } // EF Core

    private Spot(Garage garage, Sector sector, string code, double lat, double lng)
    {
        Garage = garage;
        Sector = sector;
        Code = code;
        Lat = lat;
        Lng = lng;
        IsDeleted = false;
    }

    internal static Spot Create(Garage garage, Sector sector, string code, double lat, double lng)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Spot code cannot be empty.", nameof(code));

        return new Spot(garage, sector, code, lat, lng);
    }

    public void SoftDelete() => IsDeleted = true;
}
