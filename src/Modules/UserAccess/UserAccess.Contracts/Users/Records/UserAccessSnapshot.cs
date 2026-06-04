namespace UserAccess.Contracts.Users.Records;

/// <summary>
/// Represents the minimal user information exposed to other internal modules.
/// / Representa as informações mínimas do usuário expostas para outros módulos internos.
/// </summary>
public record UserAccessSnapshot( 
    Guid UserId,
    bool IsActive,
    bool IsProvider);