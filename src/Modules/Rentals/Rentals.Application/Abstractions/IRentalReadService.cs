using Rentals.Application.GetRentals.ReadModels;

namespace Rentals.Application.Abstractions;

/// <summary>
/// Defines optimized rental read operations.
/// / Define operações otimizadas de leitura de aluguéis.
/// </summary>
public interface IRentalReadService
{
    Task<RentalSearchPage> SearchAsync(
        RentalSearchCriteria criteria,
        CancellationToken cancellationToken);
    
    Task<RentalReadModel?> GetByIdForParticipantAsync(
        Guid rentalId,
        Guid participantId,
        CancellationToken cancellationToken);
}