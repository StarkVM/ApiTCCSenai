namespace UserAccess.Domain.Interfaces;

/// <summary>
/// Service responsible for validating CPF identity data.
/// / Serviço responsável por validar dados de identidade por CPF.
/// </summary>
public interface ICpfValidator
{
    Task<bool> ValidateAsync(
        string cpf,
        string fullName,
        DateOnly birthDate,
        CancellationToken cancellationToken);
}