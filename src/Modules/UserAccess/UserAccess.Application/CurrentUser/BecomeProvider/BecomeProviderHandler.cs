using Microsoft.Extensions.Logging;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Application.CurrentUser.BecomeProvider.Records;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.CurrentUser.BecomeProvider;

public sealed class BecomeProviderHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<BecomeProviderHandler> _logger;

    public BecomeProviderHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<BecomeProviderHandler> logger,
        IClock clock)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _clock = clock;
    }

    public async Task<BecomeProviderResult> HandleAsync(
        BecomeProviderCommand command,
        CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        
        var userId = command.UserId;
        
        _logger.LogInformation(
            "Starting become provider flow for UserId: {UserId}",
            userId);

        if (!userId.GuidIdIsValid())
        {
            _logger.LogWarning(
                "Become provider failed because UserId is invalid. UserId: {UserId}",
                userId);
            throw new InvalidGuidIdException();
        }
        
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning(
                "Become provider failed because user was not found. UserId: {UserId}",
                userId);
            throw new UserNotFoundException();
        }

        user.BecomeProvider(nowUtc);
        
        _logger.LogInformation(
            "User marked as provider. UserId: {UserId}, ChangedAtUtc: {ChangedAtUtc}",
            user.Id,
            nowUtc);
        
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Become provider flow completed successfully. UserId: {UserId}",
                user.Id);
        }
        catch(Exception ex)
        {
            _logger.LogError(
                ex,
                "Become provider save failed. UserId: {UserId}",
                user.Id);
            throw new DatabaseSaveFailedException(ex);
        }
        
        return new BecomeProviderResult(true);
    }
}