using Estapar.ParkingManager.Domain.Services;
using Estapar.ParkingManager.Domain.ValueObjects;

namespace Estapar.ParkingManager.Domain.Entities;

/// <summary>
/// Raiz de agregação que representa o ciclo de estacionamento de um único veículo,
/// da ENTRY à EXIT. O fator de preço dinâmico é calculado e travado no momento da
/// entrada, com base na ocupação do setor naquele instante.
/// </summary>
public sealed class ParkingSession
{
    public Guid Id { get; private set; }
    public LicensePlate LicensePlate { get; private set; } = null!;
    public int SectorId { get; private set; }
    public DateTime EntryTime { get; private set; }
    public DateTime? ExitTime { get; private set; }
    public decimal LockedPriceFactor { get; private set; }
    public int? SpotId { get; private set; }
    public Money? AmountCharged { get; private set; }

    public bool IsOpen => ExitTime is null;

    private ParkingSession() { } // EF Core

    private ParkingSession(Guid id, LicensePlate licensePlate, int sectorId, DateTime entryTime, decimal lockedPriceFactor)
    {
        Id = id;
        LicensePlate = licensePlate;
        SectorId = sectorId;
        EntryTime = entryTime;
        LockedPriceFactor = lockedPriceFactor;
    }

    /// <summary>Abre uma nova sessão de estacionamento para o veículo, travando o fator de preço vigente no setor.</summary>
    public static ParkingSession Open(LicensePlate licensePlate, int sectorId, DateTime entryTime, decimal lockedPriceFactor)
    {
        if (sectorId <= 0)
            throw new ArgumentOutOfRangeException(nameof(sectorId), "O Id do setor deve ser maior que zero.");

        return new ParkingSession(Guid.NewGuid(), licensePlate, sectorId, entryTime, lockedPriceFactor);
    }

    /// <summary>Atribui a vaga física ocupada pelo veículo à sessão em aberto.</summary>
    public void AssignSpot(int spotId)
    {
        if (!IsOpen)
            throw new InvalidOperationException($"Não é possível atribuir uma vaga à sessão {Id}: ela já está encerrada.");

        SpotId = spotId;
    }

    /// <summary>Encerra a sessão, calculando o valor cobrado a partir do preço base do setor e do fator travado na entrada.</summary>
    public void Close(DateTime exitTime, decimal sectorBasePrice)
    {
        if (!IsOpen)
            throw new InvalidOperationException($"A sessão {Id} já está encerrada.");

        AmountCharged = FeeCalculator.Calculate(EntryTime, exitTime, sectorBasePrice, LockedPriceFactor);
        ExitTime = exitTime;
    }
}
