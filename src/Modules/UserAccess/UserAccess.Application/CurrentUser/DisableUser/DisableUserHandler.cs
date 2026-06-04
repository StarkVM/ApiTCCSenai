using Microsoft.Extensions.Logging;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Application.CurrentUser.DisableUser.Records;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.CurrentUser.DisableUser;

public sealed class DisableUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<DisableUserHandler> _logger;

    public DisableUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<DisableUserHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<DisableUserResult> HandleAsync(
        DisableUserCommand command,
        CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        var userId = command.UserId;

        if (!userId.GuidIdIsValid())
        {
            _logger.LogWarning(
                "Disable user failed because UserId is invalid. UserId: {UserId}",
                userId);
            throw new InvalidGuidIdException();
        }
        
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        user.Disable(nowUtc);

        _logger.LogInformation(
            "User disabled. UserId: {UserId}, ChangedAtUtc: {ChangedAtUtc}",
            user.Id,
            nowUtc);
        
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Disable user flow completed successfully. UserId: {UserId}",
                user.Id);
        }
        catch(Exception ex)
        {
            _logger.LogError(
                ex,
                "Disable user save failed. UserId: {UserId}",
                user.Id);
            throw new DatabaseSaveFailedException(ex);
        }
        
        return new DisableUserResult(true);
    }
}