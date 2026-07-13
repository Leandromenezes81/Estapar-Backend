namespace Estapar.ParkingManager.Domain.Services;

/// <summary>
/// Tabela de precificação dinâmica aplicada ao preço base de um setor, com base na
/// sua taxa de ocupação no momento em que um veículo entra.
/// </summary>
public static class PricingPolicy
{
    private const decimal LowOccupancyThreshold = 0.25m;
    private const decimal NormalOccupancyThreshold = 0.50m;
    private const decimal HighOccupancyThreshold = 0.75m;

    private const decimal DiscountFactor = 0.90m;
    private const decimal NormalFactor = 1.00m;
    private const decimal SurchargeFactor = 1.10m;
    private const decimal HighSurchargeFactor = 1.25m;

    /// <summary>Calcula o fator de preço aplicável para a taxa de ocupação informada.</summary>
    /// <param name="occupancyRatio">Taxa de vagas ocupadas em relação à capacidade do setor, no intervalo [0, 1].</param>
    public static decimal CalculateFactor(decimal occupancyRatio)
    {
        if (occupancyRatio < 0)
            throw new ArgumentOutOfRangeException(nameof(occupancyRatio), "A taxa de ocupação não pode ser negativa.");

        return occupancyRatio switch
        {
            < LowOccupancyThreshold => DiscountFactor,
            <= NormalOccupancyThreshold => NormalFactor,
            <= HighOccupancyThreshold => SurchargeFactor,
            _ => HighSurchargeFactor
        };
    }
}
