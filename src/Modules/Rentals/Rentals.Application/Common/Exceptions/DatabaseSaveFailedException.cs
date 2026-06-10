namespace Rentals.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a database save operation fails.
/// / Exceção lançada quando uma operação de salvamento no banco falha.
/// </summary>
public sealed class DatabaseSaveFailedException : Exception
{
    public DatabaseSaveFailedException(Exception? innerException = null)
        : base("Database save failed.", innerException)
    {
    }
}