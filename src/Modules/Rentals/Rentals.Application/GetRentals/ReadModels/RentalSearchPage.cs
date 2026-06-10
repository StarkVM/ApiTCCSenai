namespace Rentals.Application.GetRentals.ReadModels;

/// <summary>
/// Represents a page of rental read models.
/// / Representa uma página de modelos de leitura de aluguéis.
/// </summary>
public sealed record RentalSearchPage(
    IReadOnlyCollection<RentalReadModel> Items,
    int TotalCount
);