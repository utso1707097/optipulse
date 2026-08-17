using Microsoft.EntityFrameworkCore;

namespace OptiPulse.IdentityAccess;

public sealed class RefreshTokenStore(IdentityDbContext dbContext) : IRefreshTokenStore
{
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokens.AddAsync(token, cancellationToken);

    public Task<RefreshToken?> FindByRawValueAsync(string rawValue, CancellationToken cancellationToken = default)
    {
        var hash = RefreshToken.ComputeHash(rawValue);
        return dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
    }

    public async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var family = await dbContext.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in family)
        {
            token.Revoke(now);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
