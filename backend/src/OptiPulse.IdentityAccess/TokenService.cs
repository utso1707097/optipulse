using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OptiPulse.SharedKernel;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// Custom JWT issuance + refresh rotation, contained entirely in the backend
/// (Constitution Principle VI). No external identity provider.
/// </summary>
public sealed class TokenService(
    IdentityDbContext dbContext,
    IRefreshTokenStore refreshTokenStore,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public async Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        // Generic failure for both unknown-user and wrong-credential cases so the
        // API cannot be used to enumerate accounts (contracts/auth-api.md).
        if (user is null || !user.CanAuthenticateWith(password ?? string.Empty))
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        var tokens = await IssueTokensAsync(user, familyId: null, cancellationToken);
        await refreshTokenStore.SaveChangesAsync(cancellationToken);
        return tokens;
    }

    public async Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is required.");

        var now = timeProvider.GetUtcNow();
        var stored = await refreshTokenStore.FindByRawValueAsync(refreshToken, cancellationToken);

        if (stored is null)
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is not recognized.");

        // REUSE DETECTION (research R10): an already-revoked token being presented
        // means it leaked — a legitimate client only ever holds the newest token in
        // the family. Revoke the whole family so both the attacker and the victim's
        // session are cut off, forcing a fresh login.
        if (stored.RevokedAt is not null)
        {
            await refreshTokenStore.RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
            await refreshTokenStore.SaveChangesAsync(cancellationToken);
            return Error.Unauthorized("Auth.RefreshTokenReused",
                "Refresh token has already been used; the session family has been revoked.");
        }

        if (!stored.IsActive(now))
            return Error.Unauthorized("Auth.RefreshTokenExpired", "Refresh token has expired.");

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken);
        if (user is null || user.Status != UserStatus.Active)
            return Error.Unauthorized("Auth.UserInactive", "User is no longer active.");

        // Rotate: revoke the presented token, issue a replacement in the same family.
        stored.Revoke(now);
        var tokens = await IssueTokensAsync(user, stored.FamilyId, cancellationToken);
        await refreshTokenStore.SaveChangesAsync(cancellationToken);
        return tokens;
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Success(); // idempotent: nothing to revoke

        var stored = await refreshTokenStore.FindByRawValueAsync(refreshToken, cancellationToken);
        if (stored is not null)
        {
            await refreshTokenStore.RevokeFamilyAsync(stored.FamilyId, timeProvider.GetUtcNow(), cancellationToken);
            await refreshTokenStore.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    private async Task<AuthTokens> IssueTokensAsync(User user, Guid? familyId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var access = CreateAccessToken(user, now);

        var (refreshEntity, rawRefresh) = RefreshToken.Issue(
            user.Id, now, TimeSpan.FromDays(_options.RefreshTokenDays), familyId);
        await refreshTokenStore.AddAsync(refreshEntity, cancellationToken);

        return new AuthTokens(access, rawRefresh, _options.AccessTokenMinutes * 60, user.Role);
    }

    private string CreateAccessToken(User user, DateTimeOffset now)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims per contracts/auth-api.md. The role claim drives RBAC policy
        // evaluation server-side; clients MUST NOT parse it for authorization.
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("name", user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("email", user.Email),
        ];

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(_options.AccessTokenMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
