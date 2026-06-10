namespace Rentals.Application.GetRentals.Enums;

/// <summary>
/// Represents the status group used to filter rentals.
/// / Representa o grupo de status utilizado para filtrar aluguéis.
/// </summary>
public enum RentalStatusFilter
{
    All = 0,
    
    Active = 1,
    
    Completed = 2,
    
    Cancelled = 3
}