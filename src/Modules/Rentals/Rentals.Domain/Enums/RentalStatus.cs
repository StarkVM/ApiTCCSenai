namespace Rentals.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a rental.
/// / Representa o status do ciclo de vida de um aluguel.
/// </summary>
public enum RentalStatus
{
    Approved = 1,
    
    InProgress = 2,
    
    Completed = 3,
    
    Cancelled = 4
}