using Estapar.ParkingManager.Domain.Enums;

namespace Estapar.ParkingManager.Domain.Entities;

/// <summary>A single physical parking spot belonging to a sector.</summary>
public sealed class Spot
{
    public int Id { get; private set; }
    public int SectorId { get; private set; }
    public double Lat { get; private set; }
    public double Lng { get; private set; }
    public SpotStatus Status { get; private set; } = SpotStatus.AVAILABLE;

    private Spot() { } // EF Core

    private Spot(int id, int sectorId, double lat, double lng)
    {
        Id = id;
        SectorId = sectorId;
        Lat = lat;
        Lng = lng;
        Status = SpotStatus.AVAILABLE;
    }

    public static Spot Create(int id, int sectorId, double lat, double lng)
    {
        if (sectorId <= 0)
            throw new ArgumentOutOfRangeException(nameof(sectorId), "O Id do setor deve ser maior que zero.");

        return new Spot(id, sectorId, lat, lng);
    }

    public bool IsAvailable => Status == SpotStatus.AVAILABLE;

    public void Occupy()
    {
        if (Status == SpotStatus.OCCUPIED)
            throw new InvalidOperationException($"A vaga {Id} já está ocupada.");

        Status = SpotStatus.OCCUPIED;
    }

    public void Release()
    {
        Status = SpotStatus.AVAILABLE;
    }
}
