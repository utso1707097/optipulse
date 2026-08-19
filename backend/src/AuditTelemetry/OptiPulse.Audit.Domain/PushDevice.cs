namespace OptiPulse.Audit.Domain;

public enum DevicePlatform
{
    Ios = 1,
    Android = 2,
}

/// <summary>
/// A device registered to receive push alerts (FR-026).
///
/// <para><b>The token is stored in the clear, and that is a deliberate exception to how this
/// system treats credentials elsewhere.</b> Passwords are PBKDF2 hashes and service-account keys
/// are SHA-256 hashes because the server only ever needs to VERIFY them. A push token is
/// different in kind: the server has to present it to FCM/APNs to address the device, so a hash
/// would make it useless. It is a routing address that happens to be unguessable, not a secret
/// the holder proves knowledge of.</para>
///
/// <para>What that costs, stated rather than glossed: a database leak lets an attacker send
/// notifications to registered devices. It does not let them read anything, and revoking a token
/// is a single row update. That is an acceptable trade for a capability the feature cannot work
/// without — unlike storing a password in the clear, which buys nothing.</para>
///
/// <para>Registration is idempotent on the token: a reinstall issues a new token, but a device
/// that re-registers the SAME token must not accumulate rows, or one alert becomes five
/// notifications to one phone.</para>
/// </summary>
public sealed class PushDevice
{
    private PushDevice() { } // EF

    private PushDevice(Guid id, Guid userId, DevicePlatform platform, string token, DateTimeOffset registeredAt)
    {
        Id = id;
        UserId = userId;
        Platform = platform;
        Token = token;
        RegisteredAt = registeredAt;
        LastSeenAt = registeredAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DevicePlatform Platform { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    public static PushDevice Register(Guid userId, DevicePlatform platform, string token, DateTimeOffset now)
        => new(Guid.CreateVersion7(), userId, platform, token, now);

    public void Touch(DateTimeOffset now)
    {
        LastSeenAt = now;
        RevokedAt = null; // re-registering revives a device that was previously retired
    }

    /// <summary>
    /// Retires the device. Called when the push provider reports the token as permanently
    /// invalid — sending to a dead token forever is how a notification backlog turns into a
    /// rate-limit problem for the tokens that still work.
    /// </summary>
    public void Revoke(DateTimeOffset now) => RevokedAt = now;
}
