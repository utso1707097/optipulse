using FluentAssertions;
using OptiPulse.IdentityAccess;
using Xunit;

namespace OptiPulse.UnitTests.Auth;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_ProducesADifferentHashEachTime_DueToPerHashSalt()
    {
        var a = PasswordHasher.Hash("correct-horse-battery");
        var b = PasswordHasher.Hash("correct-horse-battery");

        a.Should().NotBe(b, "each hash must use a fresh random salt");
    }

    [Fact]
    public void Verify_AcceptsTheCorrectPassword()
    {
        var hash = PasswordHasher.Hash("correct-horse-battery");

        PasswordHasher.Verify("correct-horse-battery", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_RejectsAnIncorrectPassword()
    {
        var hash = PasswordHasher.Hash("correct-horse-battery");

        PasswordHasher.Verify("wrong-password", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-valid-format")]
    [InlineData("999.notbase64!.alsonotbase64!")]
    [InlineData("0.c2FsdA==.aGFzaA==")]
    public void Verify_ReturnsFalseAndNeverThrows_OnMalformedStoredHash(string storedHash)
    {
        // A corrupt credential record must not crash the login path.
        var act = () => PasswordHasher.Verify("any-password", storedHash);

        act.Should().NotThrow();
        act().Should().BeFalse();
    }

    [Fact]
    public void Hash_EmbedsTheIterationCount_SoTheWorkFactorCanBeRaisedLater()
    {
        var hash = PasswordHasher.Hash("correct-horse-battery");

        var parts = hash.Split('.');
        parts.Should().HaveCount(3);
        int.Parse(parts[0]).Should().BeGreaterThan(100_000, "OWASP-grade PBKDF2 work factor");
    }
}
