namespace Estapar.ParkingManager.Application.Interfaces;

/// <summary>Abstrai a persistência das alterações feitas em um caso de uso em uma única transação.</summary>
public interface IUnitOfWork
{
    /// <summary>Persiste todas as alterações pendentes no contexto de dados.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
