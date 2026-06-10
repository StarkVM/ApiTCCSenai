namespace Rentals.Domain.Interfaces;

/// <summary>
/// Represents the transaction boundary for the Rentals module.
/// / Representa o limite transacional do módulo de aluguéis.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}