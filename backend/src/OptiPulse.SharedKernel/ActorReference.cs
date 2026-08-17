namespace OptiPulse.SharedKernel;

/// <summary>
/// Cross-context reference to the acting principal, by ID only (data-model.md:
/// "Cross-context links are by ID only; each context persists and validates its
/// own aggregates"). Identity & Access owns the full User/RefreshToken aggregates;
/// every other context only ever needs this lightweight attribution reference.
/// </summary>
public readonly record struct ActorReference(Guid ActorId, ActorRole Role, string DisplayName)
{
    public static readonly ActorReference System = new(Guid.Empty, ActorRole.Service, "system");
}

public enum ActorRole
{
    Manager,
    Admin,
    Service,
}
