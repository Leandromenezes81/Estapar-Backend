namespace Estapar.ParkingManager.Domain.Exceptions;

public sealed class SessionNotFoundException : Exception
{
    public string LicensePlate { get; }

    public SessionNotFoundException(string licensePlate)
        : base($"No open parking session found for license plate '{licensePlate}'.")
    {
        LicensePlate = licensePlate;
    }
}
