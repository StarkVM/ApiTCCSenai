using UserAccess.Domain.Enums;

namespace UserAccess.Application.CurrentUser.Me.Records;

public record MeResult(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Email,
    UserStatus Status,
    UserType Type,
    DateTime CreatedAt,
    AddressResult Address
    );