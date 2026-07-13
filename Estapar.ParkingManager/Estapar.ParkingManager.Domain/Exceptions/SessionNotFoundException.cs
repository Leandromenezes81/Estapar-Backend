namespace Estapar.ParkingManager.Domain.Exceptions;

public sealed class SessionNotFoundException : Exception
{
    public string LicensePlate { get; }

    public SessionNotFoundException(string licensePlate)
        : base($"Nenhuma sessão de estacionamento em aberto encontrada para a placa '{licensePlate}'.")
    {
        LicensePlate = licensePlate;
    }
}
