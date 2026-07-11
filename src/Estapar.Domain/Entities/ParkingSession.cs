using Estapar.Domain.Services;
using Estapar.Domain.ValueObjects;

namespace Estapar.Domain.Entities;

/// <summary>
/// Aggregate root representing a single vehicle's parking cycle, from
/// ENTRY to EXIT. The dynamic price factor is calculated and locked in
/// at entry time, based on the sector's occupancy at that moment.
/// </summary>
public sealed class ParkingSession
{
    public Guid Id { get; private set; }
    public LicensePlate LicensePlate { get; private set; } = null!;
    public string SectorName { get; private set; } = null!;
    public DateTime EntryTime { get; private set; }
    public DateTime? ExitTime { get; private set; }
    public decimal LockedPriceFactor { get; private set; }
    public int? SpotId { get; private set; }
    public Money? AmountCharged { get; private set; }

    public bool IsOpen => ExitTime is null;

    private ParkingSession() { } // EF Core

    private ParkingSession(Guid id, LicensePlate licensePlate, string sectorName, DateTime entryTime, decimal lockedPriceFactor)
    {
        Id = id;
        LicensePlate = licensePlate;
        SectorName = sectorName;
        EntryTime = entryTime;
        LockedPriceFactor = lockedPriceFactor;
    }

    public static ParkingSession Open(LicensePlate licensePlate, string sectorName, DateTime entryTime, decimal lockedPriceFactor)
    {
        if (string.IsNullOrWhiteSpace(sectorName))
            throw new ArgumentException("Sector name cannot be empty.", nameof(sectorName));

        return new ParkingSession(Guid.NewGuid(), licensePlate, sectorName, entryTime, lockedPriceFactor);
    }

    public void AssignSpot(int spotId)
    {
        if (!IsOpen)
            throw new InvalidOperationException($"Cannot assign a spot to session {Id}: it is already closed.");

        SpotId = spotId;
    }

    public void Close(DateTime exitTime, decimal sectorBasePrice)
    {
        if (!IsOpen)
            throw new InvalidOperationException($"Session {Id} is already closed.");

        AmountCharged = FeeCalculator.Calculate(EntryTime, exitTime, sectorBasePrice, LockedPriceFactor);
        ExitTime = exitTime;
    }
}
