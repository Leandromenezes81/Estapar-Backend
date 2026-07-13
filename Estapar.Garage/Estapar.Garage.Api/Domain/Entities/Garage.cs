namespace Estapar.Garage.Api.Domain.Entities;

/// <summary>
/// Raiz de agregação: uma garagem física, suas divisões lógicas de precificação/capacidade
/// (Sectors) e as vagas individuais (Spots) que pertencem a ela.
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

    /// <summary>Cria uma nova garagem com o nome informado.</summary>
    public static Garage Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Garage name cannot be empty.", nameof(name));

        return new Garage(name);
    }

    /// <summary>Altera o nome da garagem.</summary>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Garage name cannot be empty.", nameof(name));

        Name = name;
    }

    /// <summary>Adiciona um novo setor a esta garagem.</summary>
    public Sector AddSector(string name, decimal basePrice, int maxCapacity)
    {
        var sector = Sector.Create(this, name, basePrice, maxCapacity);
        _sectors.Add(sector);
        return sector;
    }

    /// <summary>Adiciona uma nova vaga a um setor desta garagem, validando que o setor pertence a ela.</summary>
    public Spot AddSpot(Sector sector, string code, double lat, double lng)
    {
        if (!ReferenceEquals(sector, _sectors.Find(s => s == sector)))
            throw new InvalidOperationException("Sector does not belong to this garage.");

        var spot = Spot.Create(this, sector, code, lat, lng);
        _spots.Add(spot);
        return spot;
    }

    /// <summary>Marca a garagem e todos os seus setores/vagas como removidos, sem excluí-los fisicamente.</summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        foreach (var sector in _sectors) sector.SoftDelete();
        foreach (var spot in _spots) spot.SoftDelete();
    }

    /// <summary>
    /// Substitui todo o conjunto de setores/vagas: remove (soft delete) tudo que está
    /// vinculado atualmente e recria a partir de <paramref name="sectors"/>. Mais simples
    /// e seguro do que reconciliar as entidades filhas uma a uma para um CRUD restrito ao agregado.
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
