namespace OptiPulse.IdentityAccess;

/// <summary>Persistence port for revocable refresh tokens (research R10).</summary>
public interface IRefreshTokenStore
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task<RefreshToken?> FindByRawValueAsync(string rawValue, CancellationToken cancellationToken = default);

    /// <summary>Revokes every token in a rotation family — used on logout and on
    /// reuse detection (a rotated token being presented again).</summary>
    Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
