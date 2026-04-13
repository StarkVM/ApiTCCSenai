namespace UserAccess.Domain.Enums;

public enum UserStatus
{
    PendingEmailVerification = 0,
    PendingIdentityVerification = 1,
    Active = 2,
    Disabled = 3   
}