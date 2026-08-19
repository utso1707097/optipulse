using FluentAssertions;
using OptiPulse.Evaluation.Domain.Hashing;
using Xunit;

namespace OptiPulse.UnitTests.Evaluation;

public sealed class BucketingTests
{
    [Fact]
    public void ComputeBucket_SameInputs_AlwaysReturnsSameBucket()
    {
        // FR-001 determinism (SC-003): identical inputs must always yield the same result.
        for (int i = 0; i < 100; i++)
        {
            var b1 = MurmurHash3.ComputeBucket("checkout.new-cta", "salt-1", "user-42");
            var b2 = MurmurHash3.ComputeBucket("checkout.new-cta", "salt-1", "user-42");
            b1.Should().Be(b2);
        }
    }

    [Fact]
    public void ComputeBucket_IsWithinBasisPointRange()
    {
        for (int i = 0; i < 10_000; i++)
        {
            var bucket = MurmurHash3.ComputeBucket("flag-x", "salt", $"user-{i}");
            bucket.Should().BeInRange(0, 9_999);
        }
    }

    [Fact]
    public void ComputeBucket_DifferentContextKeys_ProduceStatisticallyUniformDistribution()
    {
        // SC-003: for a 50% rollout across 10,000 distinct contexts, the realized
        // enabled share must fall within +/-1 percentage point of the target.
        const int sampleSize = 10_000;
        const int rolloutBasisPoints = 5_000; // 50%
        int enabledCount = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            var bucket = MurmurHash3.ComputeBucket("checkout.new-cta", "rollout-salt", $"user-{i}");
            if (bucket < rolloutBasisPoints)
                enabledCount++;
        }

        double enabledShare = enabledCount / (double)sampleSize;
        enabledShare.Should().BeApproximately(0.50, 0.01);
    }

    [Fact]
    public void ComputeBucket_AcrossManyFlagKeys_IsUniformOnAverage()
    {
        // The uniformity claim in SC-003 is about bucketing in general, not about one lucky
        // flag key — and a single 10,000-sample draw cannot carry it. The standard error of
        // that proportion is 0.5%, so asserting +/-1% on one draw is a two-sigma test that
        // fails about one run in twenty. The integration test used to do exactly that with a
        // random key, and flaked accordingly.
        //
        // Here the sample is pooled across 200 distinct flag keys, so n = 400,000 and the
        // standard error falls to roughly 0.08%. The 0.5% bound below is therefore about six
        // sigma: comfortably deterministic in practice, while still tight enough that a
        // genuinely skewed hash could not pass.
        const int flagKeys = 200;
        const int contextsPerKey = 2_000;
        const int rolloutBasisPoints = 5_000; // 50%

        int enabledCount = 0;
        int worstKeyDeviationBp = 0;

        for (int f = 0; f < flagKeys; f++)
        {
            int enabledForKey = 0;
            for (int i = 0; i < contextsPerKey; i++)
            {
                var bucket = MurmurHash3.ComputeBucket($"flag-{f}", "rollout-salt", $"user-{i}");
                if (bucket < rolloutBasisPoints)
                    enabledForKey++;
            }

            enabledCount += enabledForKey;

            int deviationBp = Math.Abs((enabledForKey * 10_000 / contextsPerKey) - 5_000);
            worstKeyDeviationBp = Math.Max(worstKeyDeviationBp, deviationBp);
        }

        double pooledShare = enabledCount / (double)(flagKeys * contextsPerKey);
        pooledShare.Should().BeApproximately(0.50, 0.005);

        // No individual key may be wildly skewed either. This bound is deliberately loose —
        // a 2,000-sample draw has a 1.1% standard error, so anything tighter would reintroduce
        // the flakiness this test exists to replace. It catches gross bias, not noise.
        worstKeyDeviationBp.Should().BeLessThan(600);
    }

    [Fact]
    public void ComputeBucket_DifferentSalt_ChangesBucketAssignment()
    {
        // Changing the salt reshuffles buckets (data-model.md: regenerating salt reshuffles).
        var withSaltA = MurmurHash3.ComputeBucket("flag-x", "salt-a", "user-1");
        var withSaltB = MurmurHash3.ComputeBucket("flag-x", "salt-b", "user-1");

        // Not a hard guarantee for any single input, but across many contexts the
        // assignments should differ meaningfully — sample many to avoid flakiness.
        int differing = 0;
        for (int i = 0; i < 1000; i++)
        {
            var a = MurmurHash3.ComputeBucket("flag-x", "salt-a", $"user-{i}");
            var b = MurmurHash3.ComputeBucket("flag-x", "salt-b", $"user-{i}");
            if (a != b) differing++;
        }

        differing.Should().BeGreaterThan(900); // overwhelming majority should differ
    }

    [Fact]
    public void Hash32_EmptyInput_DoesNotThrow()
    {
        var act = () => MurmurHash3.Hash32(ReadOnlySpan<byte>.Empty);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(17)]
    public void Hash32_HandlesAllTailLengths(int byteLength)
    {
        var data = new byte[byteLength];
        for (int i = 0; i < byteLength; i++) data[i] = (byte)(i + 1);

        var act = () => MurmurHash3.Hash32(data);
        act.Should().NotThrow();
    }
}
