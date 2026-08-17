using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OptiPulse.Api;
using OptiPulse.Api.Auth;
using OptiPulse.Api.Endpoints;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Infrastructure;
using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Infrastructure;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Infrastructure;
using OptiPulse.IdentityAccess;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

// Bootstrap logger so startup failures before the host is built are still logged
// (standard Serilog pattern) — Constitution v2.1.0 observability baseline.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateSlimBuilder(args);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, EvaluationJsonContext.Default);
        options.SerializerOptions.TypeInfoResolverChain.Insert(1, AuthJsonContext.Default);
    });

    builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console());

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("OptiPulse.Api"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter());

    builder.Services.AddProblemDetails();

    // Native OpenAPI (Constitution Principle VII — no Swashbuckle).
    builder.Services.AddOpenApi();

    // Both reads deferred inside the configure-lambda (not hoisted to an outer
    // variable) so they observe the SAME configuration snapshot — including any
    // test-host overrides (WebApplicationFactory.ConfigureWebHost), which are
    // injected after this point in the pipeline but before the lambda actually
    // runs. Reading `Database:Provider` eagerly here previously captured a
    // stale value while the connection string (read lazily) picked up the
    // override, causing the provider and connection string to mismatch.
    builder.Services.AddDbContext<FlagsDbContext>(options => ConfigureProvider(
        options,
        builder.Configuration["Database:Provider"] ?? "Sqlite",
        builder.Configuration.GetConnectionString("Flags") ?? "Data Source=optipulse-flags.db"));

    builder.Services.AddDbContext<AuditDbContext>(options => ConfigureProvider(
        options,
        builder.Configuration["Database:Provider"] ?? "Sqlite",
        builder.Configuration.GetConnectionString("Audit") ?? "Data Source=optipulse-audit.db"));

    builder.Services.AddDbContext<IdentityDbContext>(options => ConfigureProvider(
        options,
        builder.Configuration["Database:Provider"] ?? "Sqlite",
        builder.Configuration.GetConnectionString("Identity") ?? "Data Source=optipulse-identity.db"));

    // Custom JWT authentication + RBAC policies (Principle VI).
    builder.Services.AddOptiPulseAuth(builder.Configuration, builder.Environment.IsDevelopment());
    builder.Services.AddOptiPulseVersioning();

    builder.Services.AddScoped<IFlagConfigurationReader, FlagConfigurationReader>();
    builder.Services.AddScoped<IAuditLog, AuditLog>();
    builder.Services.AddScoped<IExposureAggregator, ExposureAggregator>();

    builder.Services.AddOptiPulseRedis(builder.Configuration);
    builder.Services.AddOptiPulseResilience();

    // Evaluation Engine: snapshot store (lock-free, atomically-swapped) bound to
    // both its read (ISnapshotStore) and write (ISnapshotWriter) ports as ONE
    // singleton instance — Program.cs is the only place that needs the concrete
    // type; every other consumer depends on the narrow interface it needs.
    builder.Services.AddSingleton<SnapshotStore>();
    builder.Services.AddSingleton<ISnapshotStore>(sp => sp.GetRequiredService<SnapshotStore>());
    builder.Services.AddSingleton<ISnapshotWriter>(sp => sp.GetRequiredService<SnapshotStore>());
    builder.Services.AddSingleton<IEvaluator, Evaluator>();

    // Composition-root bridge from Flag Management to the Evaluation Engine
    // (see FlagCompilationAdapter.cs for why this lives here, not in either context).
    builder.Services.AddScoped<IFlagConfigurationProvider, FlagCompilationAdapter>();
    builder.Services.AddHostedService<InvalidationSubscriber>();

    // Exposure pipeline: one singleton writer (bound to both the concrete type,
    // for the drain service, and IExposureRecorder, for evaluation callers).
    builder.Services.AddSingleton<ExposureWriter>();
    builder.Services.AddSingleton<IExposureRecorder>(sp => sp.GetRequiredService<ExposureWriter>());
    builder.Services.AddHostedService<ExposureDrainService>();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // One shared version set for every group, so introducing v2 later is a change in
    // ApiVersioning rather than a sweep of hardcoded route strings (Principle VII).
    var versionSet = app.CreateVersionSet();
    app.MapAuthEndpoints(versionSet);
    app.MapEvaluationEndpoints(versionSet);

    await BootstrapAsync(app);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "OptiPulse.Api terminated unexpectedly during startup");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureProvider(DbContextOptionsBuilder options, string provider, string connectionString)
{
    // Migrations are authored against SQLite (the dev-time provider, per the
    // constitution's SQLite-dev / Postgres-prod split). Npgsql's conventions
    // compute a slightly different model snapshot for the same C# model (e.g.
    // provider-specific type mappings), so EF raises PendingModelChangesWarning
    // when applying those migrations on Postgres even though the schema is
    // correct. Suppressing this specific warning is EF's documented knob for
    // multi-provider setups; it does NOT suppress real migration failures, which
    // still throw. Provider-specific migration sets (a Phase 5+ concern once the
    // schema stabilises) would remove the need for this — see the KNOWN LIMITATION
    // note on BootstrapAsync below.
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

    if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
    }
    else
    {
        options.UseSqlite(connectionString);
    }
}

/// <summary>
/// Prepares the database schema and loads the initial flag snapshot (research R2
/// bootstrap) before the host starts accepting traffic.
///
/// KNOWN LIMITATION (tracked for Phase 5+): the committed migration set is
/// SQLite-authored, and EF Core migrations are provider-specific — SQLite emits
/// INTEGER for bool/Guid where PostgreSQL needs boolean/uuid, so replaying those
/// migrations against Postgres produces a subtly wrong schema. Until a dedicated
/// Postgres migration set exists, the schema strategy is configurable:
///   "Migrate"       — apply committed migrations (default; correct for SQLite)
///   "EnsureCreated" — generate provider-correct DDL from the model at runtime
/// Integration tests run against real Postgres with "EnsureCreated", which still
/// satisfies the constitution's "real database, never in-memory" testing rule.
/// </summary>
static async Task BootstrapAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var flagsDb = services.GetRequiredService<FlagsDbContext>();
    var auditDb = services.GetRequiredService<AuditDbContext>();
    var identityDb = services.GetRequiredService<IdentityDbContext>();

    var schemaStrategy = app.Configuration["Database:SchemaStrategy"] ?? "Migrate";
    if (string.Equals(schemaStrategy, "EnsureCreated", StringComparison.OrdinalIgnoreCase))
    {
        await EnsureSchemaAsync(flagsDb);
        await EnsureSchemaAsync(auditDb);
        await EnsureSchemaAsync(identityDb);
    }
    else
    {
        await flagsDb.Database.MigrateAsync();
        await auditDb.Database.MigrateAsync();
        await identityDb.Database.MigrateAsync();
    }

    var provider = services.GetRequiredService<IFlagConfigurationProvider>();
    var snapshotWriter = services.GetRequiredService<ISnapshotWriter>();
    var timeProvider = services.GetRequiredService<TimeProvider>();

    var flags = await provider.GetAllCompiledFlagsAsync();
    var snapshot = new OptiPulse.Evaluation.Domain.FlagSnapshot(
        version: flags.Count == 0 ? 0 : flags.Max(f => f.Version),
        builtAt: timeProvider.GetUtcNow(),
        flags: flags);

    snapshotWriter.LoadInitial(snapshot);
    app.Logger.LogInformation("Loaded initial flag snapshot: {Count} active flag(s), version {Version}", flags.Count, snapshot.Version);
}

/// <summary>
/// Creates the given context's tables, handling the multi-context-single-database
/// case that plain EnsureCreated cannot.
///
/// EnsureCreated is per-DATABASE, not per-context: once any one context has
/// created the database, subsequent contexts see "database exists" and skip table
/// creation entirely — leaving their own tables missing. Because OptiPulse's three
/// contexts deliberately share one physical database (per-context DbContext for
/// isolation, one database for operational simplicity — research R5), each context
/// must be asked to create its tables explicitly. A duplicate-table error means
/// those tables are already present, which is a benign no-op here.
/// </summary>
static async Task EnsureSchemaAsync(DbContext context)
{
    var creator = context.GetService<IRelationalDatabaseCreator>();

    if (!await creator.ExistsAsync())
    {
        await creator.CreateAsync();
    }

    try
    {
        await creator.CreateTablesAsync();
    }
    catch (DbException)
    {
        // Tables for this context already exist (another context created the
        // database first). Nothing to do.
    }
}

/// <summary>Marker type exposed for WebApplicationFactory-based integration tests
/// (tests/OptiPulse.IntegrationTests) to reference this assembly's entry point.</summary>
public partial class Program;
