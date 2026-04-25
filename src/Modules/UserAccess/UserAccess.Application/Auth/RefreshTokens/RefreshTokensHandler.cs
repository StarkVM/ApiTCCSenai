using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using UserAccess.Application.Auth.RefreshTokens.Records;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.Auth;
using UserAccess.Domain.Exceptions.Users;

namespace UserAccess.Application.Auth.RefreshTokens;

public sealed class RefreshTokensHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<RefreshTokensHandler> _logger;
    
    public RefreshTokensHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenHasher refreshTokenHasher,
        ITokenIssuer tokenIssuer,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<RefreshTokensHandler> logger
        )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenHasher = refreshTokenHasher;
        _tokenIssuer = tokenIssuer;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
        
    }

    public async Task<RefreshTokensResult> RefreshAsync(RefreshTokensCommand command, CancellationToken cancellationToken)
    {
        var refreshTokenString = command.RefreshToken?.Trim();
        
        var nowUtc = _clock.UtcNow;
        
        if (string.IsNullOrWhiteSpace(refreshTokenString))
        {
            _logger.LogWarning("Refresh token request failed: token is empty.");
            throw new ArgumentException("Refresh token is required.");
        }
        
        _logger.LogInformation("Starting user refresh tokens flow for token {Token}", refreshTokenString);
        
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshTokenString);    
        
        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);

        if (refreshToken is null)
        {
            _logger.LogWarning(
                "Refresh token not found. Token: {Token}", refreshTokenString);
            throw new RefreshTokenNotFoundException();

        }

        if (refreshToken.User.Status != UserStatus.PendingIdentityVerification &&
            refreshToken.User.Status != UserStatus.Active)
        {
            _logger.LogWarning(
                "Invalid User. Token: {Token}", refreshTokenString);
            throw new AuthInvalidUserException();
        }

        if (!refreshToken.IsActive(nowUtc))
        {
            _logger.LogWarning(
                "Refresh token is not active (expired or revoked). UserId: {UserId}.",
                refreshToken.UserId
                );

            throw new RefreshTokenNotActiveException();
        }
        
        
        var result = await _tokenIssuer.IssueAsync(refreshToken.User, cancellationToken);
        
        refreshToken.Revoke(nowUtc, result.RefreshTokenHash, "REFRESH_TOKEN_ROTATED");
        
        try
        {
            _logger.LogInformation(
                "Refresh token rotation persisted successfully for user {UserId}.",
                refreshToken.UserId
                );
           await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch(Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to persist refresh token rotation for user {UserId}.",
                refreshToken.UserId
                );
            
            throw new DatabaseSaveFailedException(exception);
        }

        return new RefreshTokensResult(
            result.AccessToken,
            result.RefreshToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshTokenExpiresAtUtc
        );
    }

    /*private void Validate(string refreshToken)
    {
       
    }*/
}