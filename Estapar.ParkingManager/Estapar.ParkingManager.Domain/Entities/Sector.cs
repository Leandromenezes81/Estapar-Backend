using Estapar.ParkingManager.Domain.Services;

namespace Estapar.ParkingManager.Domain.Entities;

/// <summary>
/// Uma divisão lógica do conjunto de vagas de uma garagem, identificada por nome
/// (ex.: "A"). Setores não são áreas físicas, apenas grupos de precificação/capacidade.
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

    /// <summary>Cria um novo setor, validando nome, preço base e capacidade máxima.</summary>
    /// <param name="id">Id do setor, vindo do <c>GET /garage</c> do Estapar.Garage.Api.</param>
    /// <param name="name">Nome do setor (ex.: "A"). Pode se repetir entre garagens diferentes.</param>
    /// <param name="basePrice">Preço base cobrado por hora no setor.</param>
    /// <param name="maxCapacity">Capacidade máxima de vagas do setor.</param>
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

    /// <summary>Calcula a taxa de ocupação (0 a 1) do setor para uma dada quantidade de vagas ocupadas.</summary>
    public decimal OccupancyRatio(int occupiedCount) => (decimal)occupiedCount / MaxCapacity;

    /// <summary>Indica se o setor está com todas as vagas ocupadas.</summary>
    public bool IsFull(int occupiedCount) => occupiedCount >= MaxCapacity;

    /// <summary>Fator de preço dinâmico para um veículo entrando enquanto <paramref name="occupiedCount"/> vagas estão ocupadas.</summary>
    public decimal PriceFactorFor(int occupiedCount) => PricingPolicy.CalculateFactor(OccupancyRatio(occupiedCount));
}
