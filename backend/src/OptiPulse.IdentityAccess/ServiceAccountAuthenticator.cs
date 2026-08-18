using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OptiPulse.IdentityAccess;

public interface IServiceAccountAuthenticator
{
    bool TryAuthenticate(string presentedKey, out Guid accountId, out string name);
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Validates SDK keys against an in-memory snapshot of active accounts.
///
/// The snapshot exists for Principle II: evaluation is the hot path, and a database round-trip
/// per request would blow the sub-5ms budget on authentication alone. Same copy-on-write shape
/// as the flag snapshot — a volatile reference swapped wholesale, so reads are lock-free.
///
/// Revocation therefore takes effect at the next refresh rather than instantly. That is a
/// deliberate trade, and it is bounded: <see cref="ServiceAccountRefreshService"/> re-reads on
/// an interval. It is acceptable here because an SDK key grants read-only evaluation — unlike
/// the kill-switch, nothing an SDK can do needs sub-second revocation. Human sessions, which
/// can mutate state, are revoked immediately through the refresh-token path instead.
/// </summary>
public sealed class ServiceAccountAuthenticator(IServiceScopeFactory scopeFactory)
    : IServiceAccountAuthenticator
{
    private volatile FrozenDictionary<string, (Guid Id, string Name)> _active =
        FrozenDictionary<string, (Guid, string)>.Empty;

    public bool TryAuthenticate(string presentedKey, out Guid accountId, out string name)
    {
        accountId = Guid.Empty;
        name = string.Empty;

        if (!ServiceAccount.LooksLikeKey(presentedKey))
            return false;

        // Hash first, then look up by the hash. No constant-time comparison is needed here and
        // none is performed: the lookup key is already a SHA-256 digest of the presented secret,
        // so any timing signal leaks information about the digest, not the key — and inverting
        // the digest to recover a 256-bit random key is the thing SHA-256 makes infeasible.
        // (A constant-time compare of a value against itself, which is easy to write here by
        // reflex, would be pure theater.)
        var hash = ServiceAccount.HashKey(presentedKey);

        if (!_active.TryGetValue(hash, out var entry))
            return false;

        accountId = entry.Id;
        name = entry.Name;
        return true;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var active = await dbContext.ServiceAccounts
            .Where(a => a.RevokedAt == null)
            .Select(a => new { a.KeyHash, a.Id, a.Name })
            .ToListAsync(cancellationToken);

        _active = active.ToFrozenDictionary(a => a.KeyHash, a => (a.Id, a.Name));
    }
}
