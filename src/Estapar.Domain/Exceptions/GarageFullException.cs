namespace Estapar.Domain.Exceptions;

public sealed class GarageFullException : Exception
{
    public string SectorName { get; }

    public GarageFullException(string sectorName)
        : base($"Sector '{sectorName}' is at full capacity. New entries are blocked.")
    {
        SectorName = sectorName;
    }
}
