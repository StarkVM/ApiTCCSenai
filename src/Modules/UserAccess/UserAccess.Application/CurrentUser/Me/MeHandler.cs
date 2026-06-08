using UserAccess.Domain.Helpers;
using Microsoft.Extensions.Logging;
using UserAccess.Application.CurrentUser.Me.Records;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.CurrentUser.Me;

public sealed class MeHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MeHandler> _logger;
    
    public MeHandler(
        IUserRepository userRepository,
        ILogger<MeHandler> logger
        )
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task<MeResult> HandleAsync(MeCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;

        if (!userId.GuidIdIsValid())
        {
            _logger.LogWarning("Invalid user id received in me request.");
            throw new InvalidUserIdException();
        }
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found for id: {UserId}", command.UserId);
            throw new UserNotFoundException();
        }
        
        if (user.Status != UserStatus.PendingIdentityVerification &&
            user.Status != UserStatus.Active)
        {
            _logger.LogWarning(
                "Invalid User. Id: {Id}", user.Id);
            throw new InvalidUserException();
        }

        if (user.Address is null)
        {
            _logger.LogWarning("Address not found for user id: {UserId}", command.UserId);
            throw new AddressNotFoundException();
        }

        var address = new AddressResult(
            user.Address.State,
            user.Address.City,
            user.Address.District,
            user.Address.Street,
            user.Address.ZipCode
        );

        return new MeResult(
            user.Id,
            user.FirstName.ToUpperInvariant(),
            user.LastName.ToUpperInvariant(),
            user.BirthDate,
            user.Email,
            user.Status,
            user.Type,
            user.CreatedAt,
                address
        );
    }
}