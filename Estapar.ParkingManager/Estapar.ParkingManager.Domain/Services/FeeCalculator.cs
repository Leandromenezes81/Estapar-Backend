using Estapar.ParkingManager.Domain.ValueObjects;

namespace Estapar.ParkingManager.Domain.Services;

/// <summary>
/// Calcula a tarifa cobrada por uma sessão de estacionamento: os primeiros 30 minutos
/// são gratuitos, e cada hora iniciada depois disso é cobrada pelo preço base do setor
/// ajustado pelo fator de preço dinâmico travado na entrada.
/// </summary>
public static class FeeCalculator
{
    private static readonly TimeSpan FreeThreshold = TimeSpan.FromMinutes(30);

    /// <summary>Calcula o valor a ser cobrado com base no tempo de permanência, no preço base do setor e no fator de preço travado.</summary>
    public static Money Calculate(DateTime entryTime, DateTime exitTime, decimal basePrice, decimal priceFactor)
    {
        if (exitTime < entryTime)
            throw new ArgumentOutOfRangeException(nameof(exitTime), "O horário de saída não pode ser anterior ao horário de entrada.");

        var duration = exitTime - entryTime;
        if (duration <= FreeThreshold)
            return Money.Zero();

        var billableHours = (int)Math.Ceiling(duration.TotalHours);
        var amount = billableHours * basePrice * priceFactor;

        return Money.Create(amount);
    }
}
