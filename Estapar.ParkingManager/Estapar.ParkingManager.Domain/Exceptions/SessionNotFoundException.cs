namespace Estapar.ParkingManager.Domain.Exceptions;

/// <summary>Exceção lançada quando não existe sessão de estacionamento em aberto para a placa informada.</summary>
public sealed class SessionNotFoundException : Exception
{
    public string LicensePlate { get; }

    /// <summary>Cria a exceção para a placa informada, já com a mensagem padrão em português.</summary>
    public SessionNotFoundException(string licensePlate)
        : base($"Nenhuma sessão de estacionamento em aberto encontrada para a placa '{licensePlate}'.")
    {
        LicensePlate = licensePlate;
    }
}
