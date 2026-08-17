using OptiPulse.SharedKernel;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// Identity &amp; Access aggregate root (data-model.md). Owns credentials and the
/// single primary role used for RBAC (FR-A03). All authentication and
/// authorization decisions happen server-side (Principle VI) — this aggregate is
/// never exposed to clients, which receive only opaque tokens.
/// </summary>
public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }

    /// <summary>EF Core materialization constructor.</summary>
    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Name = string.Empty;
    }

    private User(Guid id, string email, string passwordHash, string name, UserRole role, UserStatus status)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
        Role = role;
        Status = status;
    }

    public static Result<User> Create(string email, string password, string name, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
            return Error.Validation("User.Email.Invalid", "A valid email address is required.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return Error.Validation("User.Password.TooShort", "Password must be at least 8 characters.");
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("User.Name.Required", "Name is required.");

        return new User(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            PasswordHasher.Hash(password),
            name.Trim(),
            role,
            UserStatus.Active);
    }

    /// <summary>True when the supplied password matches AND the account is active
    /// — a disabled account must never authenticate (FR-A04).</summary>
    public bool CanAuthenticateWith(string password) =>
        Status == UserStatus.Active && PasswordHasher.Verify(password, PasswordHash);

    public ActorReference ToActorReference() => new(
        Id,
        Role == UserRole.Manager ? ActorRole.Manager : ActorRole.Admin,
        Name);
}

public enum UserRole
{
    Manager,
    Admin,
}

public enum UserStatus
{
    Active,
    Disabled,
}
