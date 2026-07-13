namespace Estapar.Garage.Api.Domain.Entities;

/// <summary>Uma vaga física individual, pertencente tanto a uma garagem quanto a um de seus setores.</summary>
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

    /// <summary>Cria uma nova vaga pertencente à garagem e ao setor informados, validando o código.</summary>
    internal static Spot Create(Garage garage, Sector sector, string code, double lat, double lng)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Spot code cannot be empty.", nameof(code));

        return new Spot(garage, sector, code, lat, lng);
    }

    /// <summary>Marca a vaga como removida, sem excluí-la fisicamente.</summary>
    public void SoftDelete() => IsDeleted = true;
}
