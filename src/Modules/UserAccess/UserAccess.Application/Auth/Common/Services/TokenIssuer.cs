using Microsoft.Extensions.Options;
using UserAccess.Application.Auth.Common.Records;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Interfaces;
using UserAccess.Application.Auth.Common.Options;

namespace UserAccess.Application.Auth.Common.Services;

public sealed class TokenIssuer
{
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly JwtOptions _jwtOptions;
    private readonly RefreshTokenOptions _refreshTokenOptions;

    public TokenIssuer(
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IOptions<JwtOptions> jwtOptions,
        IOptions<RefreshTokenOptions> refreshTokenOptions )
    {
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _jwtOptions = jwtOptions.Value;
        _refreshTokenOptions = refreshTokenOptions.Value;
    }

    public async Task<AuthTokensResult> IssueAsync(User? user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        var nowUtc = _clock.UtcNow;

        var accessTokenExpiresAtUtc = nowUtc.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshTokenExpiresAtUtc = nowUtc.AddDays(_refreshTokenOptions.RefreshTokenDays);

        var accessToken = _accessTokenGenerator.Generate(user);

        var refreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken);

        var refreshTokenEntity = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            nowUtc,
            refreshTokenExpiresAtUtc
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensResult(
            accessToken,
            refreshToken,
            refreshTokenHash,
            accessTokenExpiresAtUtc,
            refreshTokenExpiresAtUtc
        );
    }
}