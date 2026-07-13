using Estapar.ParkingManager.Domain.Enums;

namespace Estapar.ParkingManager.Domain.Entities;

/// <summary>Uma vaga física individual pertencente a um setor.</summary>
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

    /// <summary>Cria uma nova vaga disponível, validando o Id do setor ao qual ela pertence.</summary>
    public static Spot Create(int id, int sectorId, double lat, double lng)
    {
        if (sectorId <= 0)
            throw new ArgumentOutOfRangeException(nameof(sectorId), "O Id do setor deve ser maior que zero.");

        return new Spot(id, sectorId, lat, lng);
    }

    public bool IsAvailable => Status == SpotStatus.AVAILABLE;

    /// <summary>Marca a vaga como ocupada. Lança exceção se ela já estiver ocupada.</summary>
    public void Occupy()
    {
        if (Status == SpotStatus.OCCUPIED)
            throw new InvalidOperationException($"A vaga {Id} já está ocupada.");

        Status = SpotStatus.OCCUPIED;
    }

    /// <summary>Libera a vaga, tornando-a disponível novamente.</summary>
    public void Release()
    {
        Status = SpotStatus.AVAILABLE;
    }
}
