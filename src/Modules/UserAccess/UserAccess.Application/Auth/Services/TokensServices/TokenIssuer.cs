using Microsoft.Extensions.Logging;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Results;

namespace UserAccess.Application.Auth.Services.TokensServices;

public sealed class TokenIssuer : ITokenIssuer
{
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TokenIssuer> _logger;
    private readonly IClock _clock;
    private readonly IAccessTokenLifetimeProvider _jwtProvider;
    private readonly IRefreshTokenLifetimeProvider _refreshTokenProvider;

    public TokenIssuer(
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<TokenIssuer> logger,
        IClock clock,
        IAccessTokenLifetimeProvider jwtProvider,
        IRefreshTokenLifetimeProvider refreshTokenProvider)
    {
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _jwtProvider = jwtProvider;
        _refreshTokenProvider = refreshTokenProvider;
    }

    public async Task<AuthTokensResult> IssueAsync(User? user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        var nowUtc = _clock.UtcNow;

        var accessTokenExpiresAtUtc = _jwtProvider.GetExpirationDateUtc(nowUtc);
        var refreshTokenExpiresAtUtc = _refreshTokenProvider.GetExpirationDateUtc(nowUtc);

        var accessToken = _accessTokenGenerator.Generate(user);

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken);

        var refreshTokenEntity = new Domain.Entities.RefreshToken(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            nowUtc,
            refreshTokenExpiresAtUtc
        );

        try
        {
            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to persist refresh token for email {Email}.", user.Email);
            throw new DatabaseSaveFailedException();
        }
        

        return new AuthTokensResult(
            accessToken,
            refreshToken,
            refreshTokenHash,
            accessTokenExpiresAtUtc,
            refreshTokenExpiresAtUtc
        );
    }
}