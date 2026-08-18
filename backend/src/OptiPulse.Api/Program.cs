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
using OptiPulse.Resilience;
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

    // Flag Management write side (T048-T051).
    builder.Services.AddScoped<IFlagRepository, FlagRepository>();
    builder.Services.AddScoped<IInvalidationPublisher, InvalidationPublisher>();
    builder.Services.AddScoped<IFlagAuditWriter, OptiPulse.Api.Adapters.FlagAuditWriter>();
    builder.Services.AddScoped<FlagManagementService>();
    builder.Services.AddSingleton(sp => new FlagInvalidationOptions
    {
        // Publisher and subscriber must agree on the channel or invalidation silently no-ops.
        InvalidationChannel = sp.GetRequiredService<RedisOptions>().InvalidationChannel,
    });
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
    app.MapManagementEndpoints(versionSet);

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
    // NOTE: PendingModelChangesWarning is deliberately NOT suppressed any more (T093).
    // It was suppressed only because the migrations were SQLite-authored, so Npgsql
    // computed a different model snapshot and every Postgres run raised it. Now that
    // migrations are authored against Postgres — the single migrated provider — that
    // warning means what it is supposed to mean: the model drifted from the migrations
    // and someone owes a new one. Silencing it would hide exactly that.
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
/// Schema strategy is configurable per environment (constitution v2.2.0 persistence rules):
///   "Migrate"       — apply the committed PostgreSQL migrations. The production path.
///   "EnsureCreated" — generate DDL from the model at runtime. SQLite dev/edge only.
///
/// PostgreSQL is the single migrated provider: EF emits provider-specific DDL, so one
/// migration set cannot serve both (SQLite emits INTEGER for bool/Guid where PostgreSQL
/// needs boolean/uuid — that mismatch is what made `MigrateAsync` fail against Postgres
/// with `42804` before T093). Rather than maintain two sets for one model, SQLite is
/// provisioned from the model directly and never migrated.
/// </summary>
static async Task BootstrapAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var flagsDb = services.GetRequiredService<FlagsDbContext>();
    var auditDb = services.GetRequiredService<AuditDbContext>();
    var identityDb = services.GetRequiredService<IdentityDbContext>();

    var schemaStrategy = app.Configuration["Database:SchemaStrategy"] ?? "Migrate";
    var databaseProvider = app.Configuration["Database:Provider"] ?? "Sqlite";

    // Fail fast on the one combination that silently produces a WRONG schema rather than an
    // error: the committed migrations are PostgreSQL-authored (the single migrated provider),
    // so replaying them on SQLite emits DDL for the wrong dialect. SQLite is provisioned from
    // the model instead. Catching this at boot beats discovering it as mis-typed columns later.
    if (!string.Equals(databaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(schemaStrategy, "EnsureCreated", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            $"Database:Provider='{databaseProvider}' requires Database:SchemaStrategy='EnsureCreated'. " +
            "Committed EF migrations are authored against PostgreSQL and must not be applied to " +
            "another provider (constitution v2.2.0: PostgreSQL is the single migrated provider).");
    }

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
