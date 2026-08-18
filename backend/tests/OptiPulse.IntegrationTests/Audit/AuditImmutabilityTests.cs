using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Audit.Domain;
using OptiPulse.Audit.Infrastructure;
using OptiPulse.IntegrationTests.Fixtures;
using OptiPulse.SharedKernel;
using Xunit;

namespace OptiPulse.IntegrationTests.Audit;

/// <summary>
/// T081 / FR-019 — the audit trail is append-only, enforced at BOTH layers.
///
/// The application guard protects code going through the DbContext. The database trigger
/// protects everything else, which is what makes "immutable" a property of the system rather
/// than a property of the code paths we happened to think of.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class AuditImmutabilityTests(OptiPulseTestFixture fixture)
{
    private async Task<AuditEntry> AppendAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var entry = AuditEntry.Create(
            ActorReference.System, AuditChangeType.FlagCreated, Guid.NewGuid(),
            DateTimeOffset.UtcNow, null, """{"state":"original"}""");
        db.AuditEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    [Fact]
    public async Task ModifyingAnEntry_ThroughSaveChangesAsync_IsRejected()
    {
        var entry = await AppendAsync();

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var tracked = await db.AuditEntries.FirstAsync(e => e.Id == entry.Id);
        db.Entry(tracked).State = EntityState.Modified;

        var act = async () => await db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*append-only*");
    }

    [Fact]
    public async Task ModifyingAnEntry_ThroughSYNCSaveChanges_IsAlsoRejected()
    {
        // This is the bypass that existed before T081: only the async overload was guarded, so
        // an ordinary synchronous save modified audit rows without ever reaching the check — and
        // did so while every test and every gate stayed green.
        var entry = await AppendAsync();

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var tracked = await db.AuditEntries.FirstAsync(e => e.Id == entry.Id);
        db.Entry(tracked).State = EntityState.Modified;

        var act = () => db.SaveChanges();

        act.Should().Throw<InvalidOperationException>().WithMessage("*append-only*");
    }

    [Fact]
    public async Task DeletingAnEntry_IsRejected()
    {
        var entry = await AppendAsync();

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var tracked = await db.AuditEntries.FirstAsync(e => e.Id == entry.Id);
        db.AuditEntries.Remove(tracked);

        var act = async () => await db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*append-only*");
    }

    [Fact]
    public async Task RawSqlUpdate_IsRejectedByTheDatabaseItself()
    {
        // The decisive test. This bypasses the DbContext guard entirely — exactly what psql, a
        // restore, or any other service with the connection string would do. If this passes,
        // "immutable audit trail" is a claim the system does not honour.
        var entry = await AppendAsync();

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var act = async () => await db.Database.ExecuteSqlRawAsync(
            """UPDATE "AuditEntries" SET "AfterState" = '{"state":"tampered"}' WHERE "Id" = {0}""",
            entry.Id);

        await act.Should().ThrowAsync<Exception>("the database trigger must reject the update");

        // And the original content is intact.
        var reloaded = await db.AuditEntries.AsNoTracking().FirstAsync(e => e.Id == entry.Id);
        reloaded.AfterStateJson.Should().Contain("original");
    }

    [Fact]
    public async Task RawSqlDelete_IsRejectedByTheDatabaseItself()
    {
        var entry = await AppendAsync();

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        var act = async () => await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "AuditEntries" WHERE "Id" = {0}""", entry.Id);

        await act.Should().ThrowAsync<Exception>();
        (await db.AuditEntries.AsNoTracking().AnyAsync(e => e.Id == entry.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task InsertingIsStillAllowed_AppendOnlyIsNotReadOnly()
    {
        var entry = await AppendAsync();

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        (await db.AuditEntries.AnyAsync(e => e.Id == entry.Id)).Should().BeTrue();
    }
}
