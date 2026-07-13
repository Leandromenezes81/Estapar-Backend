namespace Estapar.ParkingManager.Domain.Exceptions;

public sealed class GarageFullException : Exception
{
    public string SectorName { get; }

    public GarageFullException(string sectorName)
        : base($"O setor '{sectorName}' está com capacidade máxima. Novas entradas estão bloqueadas.")
    {
        SectorName = sectorName;
    }
}
