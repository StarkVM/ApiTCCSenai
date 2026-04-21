namespace UserAccess.Domain.Enums;

/// <summary>
/// Represents the current lifecycle status of a user.
/// / Representa o estado atual do ciclo de vida do usuário.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User registered but email not verified yet.
    /// / Usuário registrado, mas email ainda não verificado.
    /// </summary>
    PendingEmailVerification = 0,
    
    /// <summary>
    /// Email verified, waiting for identity validation.
    /// / Email verificado, aguardando validação de identidade.
    /// </summary>
    PendingIdentityVerification = 1,
    
    /// <summary>
    /// Identity verification failed.
    /// / Verificação de identidade reprovada.
    /// </summary>
    IdentityDenied = 2,
    
    /// <summary>
    /// Fully active user.
    /// / Usuário ativo.
    /// </summary>
    Active = 3,
    
    /// <summary>
    /// User disabled by system or admin.
    /// / Usuário desativado.
    /// </summary>
    Disabled = 4,
}