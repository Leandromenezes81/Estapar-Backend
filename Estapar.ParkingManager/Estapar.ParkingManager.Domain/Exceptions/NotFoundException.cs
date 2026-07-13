namespace Estapar.ParkingManager.Domain.Exceptions;

/// <summary>Exceção genérica para indicar que um recurso solicitado não foi encontrado.</summary>
public class NotFoundException : Exception
{
    /// <summary>Cria a exceção com a mensagem informada.</summary>
    public NotFoundException(string message) : base(message)
    {
    }
}
