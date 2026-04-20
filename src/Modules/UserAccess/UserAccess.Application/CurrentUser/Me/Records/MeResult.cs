using UserAccess.Domain.Enums;

namespace UserAccess.Application.CurrentUser.Me.Records;

public record MeResult(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    UserStatus Status,
    UserType Type,
    AddressResult Address
    );