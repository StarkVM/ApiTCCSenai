using Microsoft.Extensions.Logging;
using UserAccess.Application.Auth.Logout.Records;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.Auth.Logout;

public sealed class LogoutCurrentSessionHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<LogoutCurrentSessionHandler> _logger;
    
    public LogoutCurrentSessionHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenHasher refreshTokenHasher,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<LogoutCurrentSessionHandler> logger
        )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenHasher = refreshTokenHasher;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<LogoutCurrentSessionResult> HandleAsync(LogoutCurrentSessionCommand command, CancellationToken cancellationToken)
    {
        var refreshTokenString = command.RefreshToken?.Trim();
        
        var nowUtc = _clock.UtcNow;
        
        _logger.LogInformation("Starting logout current session flow.");
        
        if (string.IsNullOrWhiteSpace(refreshTokenString))
        {
            _logger.LogWarning("Logout failed: token is empty.");
            throw new ArgumentException("REFRESH_TOKEN_REQUIRED");
        }
            
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshTokenString);
        
        var token = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);
        
        if (token is null)
        {
            _logger.LogWarning("Logout failed: refresh token not found.");
            throw new InvalidOperationException("REFRESH_TOKEN_NOT_FOUND");
        }
        
        _logger.LogInformation("Starting logout current session flow for UserId: {UserId}", token.UserId);
        
        if (!token.IsActive(nowUtc))
        {
            _logger.LogInformation("Logout current session ignored because refresh token is no longer active.");
            return new LogoutCurrentSessionResult(true);
        }

        token.Revoke(nowUtc, null, "USER_LOGOUT");
        
        _logger.LogInformation("Successfully logged out current session UserId: {Id} ",token.UserId);
        
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Logout current session changes saved successfully for UserId: {UserId}", token.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save data for UserId: {Id}", token.UserId);
            throw new InvalidOperationException("DB_SAVE_FAILED", ex);
        }

        return new LogoutCurrentSessionResult(true);
    }
}