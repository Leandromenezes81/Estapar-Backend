namespace Estapar.ParkingManager.Domain.Exceptions;

/// <summary>Exceção lançada quando uma tentativa de entrada é feita em um setor que já está com capacidade máxima.</summary>
public sealed class GarageFullException : Exception
{
    public string SectorName { get; }

    /// <summary>Cria a exceção para o setor informado, já com a mensagem padrão em português.</summary>
    public GarageFullException(string sectorName)
        : base($"O setor '{sectorName}' está com capacidade máxima. Novas entradas estão bloqueadas.")
    {
        SectorName = sectorName;
    }
}
