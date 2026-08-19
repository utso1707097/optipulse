using FluentAssertions;
using OptiPulse.Api;
using Xunit;

namespace OptiPulse.UnitTests.Api;

/// <summary>
/// These exist because a wrong connection string fails at startup with an opaque parse error
/// that never mentions the URL, and the operator has no way to tell a bad password from a bad
/// format. Every platform that injects a database URL is covered here.
/// </summary>
public sealed class ConnectionStringNormalizerTests
{
    [Fact]
    public void PostgresUrl_IsConvertedToNpgsqlKeyValueForm()
    {
        var result = ConnectionStringNormalizer.Postgres(
            "postgres://opti_user:s3cret@dpg-abc123.frankfurt-postgres.render.com:5432/optipulse");

        result.Should().Contain("Host=dpg-abc123.frankfurt-postgres.render.com");
        result.Should().Contain("Port=5432");
        result.Should().Contain("Database=optipulse");
        result.Should().Contain("Username=opti_user");
        result.Should().Contain("Password=s3cret");
        result.Should().Contain("SSL Mode=Require", "managed Postgres requires TLS and Npgsql does not assume it");
    }

    [Fact]
    public void PostgresqlScheme_IsAlsoAccepted()
    {
        // Neon and Heroku use `postgresql://`, Render uses `postgres://`. Both are the same thing.
        ConnectionStringNormalizer.Postgres("postgresql://u:p@host:5432/db")
            .Should().Contain("Host=host");
    }

    [Fact]
    public void PercentEncodedPassword_IsDecoded()
    {
        // Generated passwords routinely contain characters that must be escaped in a URL. Failing
        // to decode them produces an authentication failure that looks like a wrong password.
        ConnectionStringNormalizer.Postgres("postgres://u:p%40ss%3Aword@host:5432/db")
            .Should().Contain("Password=p@ss:word");
    }

    [Fact]
    public void AnAlreadyNativeConnectionString_IsLeftAlone()
    {
        const string native = "Host=localhost;Port=5432;Database=optipulse;Username=postgres;Password=postgres";

        ConnectionStringNormalizer.Postgres(native).Should().Be(native);
    }

    [Fact]
    public void SqliteConnectionString_IsUntouched()
    {
        const string sqlite = "Data Source=optipulse-flags.db";

        ConnectionStringNormalizer.Postgres(sqlite).Should().Be(sqlite);
    }

    [Fact]
    public void RedisUrl_IsConvertedToStackExchangeForm()
    {
        var result = ConnectionStringNormalizer.Redis("redis://default:tok3n@fly-cache.upstash.io:6379");

        result.Should().StartWith("fly-cache.upstash.io:6379");
        result.Should().Contain("password=tok3n");
    }

    [Fact]
    public void RedissScheme_EnablesTls()
    {
        ConnectionStringNormalizer.Redis("rediss://default:tok3n@secure.upstash.io:6380")
            .Should().Contain("ssl=True");
    }

    [Fact]
    public void PlainRedisUrl_DoesNotForceTls()
    {
        // Assuming TLS on `redis://` would break a local unencrypted instance, so only the
        // explicit `rediss` scheme turns it on.
        ConnectionStringNormalizer.Redis("redis://localhost:6379")
            .Should().NotContain("ssl=True");
    }

    [Fact]
    public void AnAlreadyNativeRedisString_IsLeftAlone()
    {
        const string native = "localhost:6379";

        ConnectionStringNormalizer.Redis(native).Should().Be(native);
    }
}
