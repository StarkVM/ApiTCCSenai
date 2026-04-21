using Microsoft.Extensions.Logging;
using UserAccess.Application.Auth.Logout.Records;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.Auth.Logout;

public sealed class LogoutAllSessionsHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<LogoutAllSessionsHandler> _logger;
    
    public LogoutAllSessionsHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenHasher refreshTokenHasher,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<LogoutAllSessionsHandler> logger
        )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<LogoutAllSessionsResult> HandleAsync(LogoutAllSessionsCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;
        
        var nowUtc = _clock.UtcNow;

        if (!userId.GuidIdIsValid())
        {
            _logger.LogWarning("Logout failed: invalid user id.");
            throw new ArgumentException("INVALID_USER_ID");
        }
        
        
        var tokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        if (tokens.Count == 0)
        {
            _logger.LogInformation("Logout all sessions ignored because no active refresh tokens were found for UserId: {UserId}.", userId);
            return new LogoutAllSessionsResult(true);
        }
        
        foreach (var t in tokens)
        {
            t.Revoke(nowUtc, null, "USER_LOGOUT");
        }
        
        _logger.LogInformation("Successfully logged out all session UserId: {Id}.",userId);
        
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Logout all sessions changes saved successfully for UserId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save data for UserId: {Id}", userId);
            throw new InvalidOperationException("DB_SAVE_FAILED", ex);
        }

        return new LogoutAllSessionsResult(true);
    }
}