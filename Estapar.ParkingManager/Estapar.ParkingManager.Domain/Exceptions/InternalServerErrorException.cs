namespace Estapar.ParkingManager.Domain.Exceptions;

/// <summary>Exceção genérica para indicar uma falha interna inesperada, mapeada para HTTP 500.</summary>
public class InternalServerErrorException : Exception
{
    /// <summary>Cria a exceção com a mensagem informada.</summary>
    public InternalServerErrorException(string message) : base(message)
    {
    }
}
