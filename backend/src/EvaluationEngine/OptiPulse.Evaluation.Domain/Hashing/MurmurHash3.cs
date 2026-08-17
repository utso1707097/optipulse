namespace OptiPulse.Evaluation.Domain.Hashing;

/// <summary>
/// Deterministic, zero-allocation MurmurHash3 (x86, 32-bit) bucketing (Constitution
/// Principle II; research R1). Operates entirely over spans — no heap allocation,
/// no LINQ, no boxing. Used to map a flag+context key to a stable [0, 10000)
/// basis-point bucket so rollout/experiment decisions are 100% reproducible across
/// nodes without any shared coordination.
/// </summary>
public static class MurmurHash3
{
    private const uint C1 = 0xcc9e2d51;
    private const uint C2 = 0x1b873593;

    /// <summary>Computes the raw 32-bit MurmurHash3 digest of <paramref name="data"/>.</summary>
    public static uint Hash32(ReadOnlySpan<byte> data, uint seed = 0)
    {
        uint hash = seed;
        int length = data.Length;
        int blockCount = length / 4;

        for (int i = 0; i < blockCount; i++)
        {
            uint k = ReadUInt32LittleEndian(data.Slice(i * 4, 4));
            hash = MixBody(hash, k);
        }

        ReadOnlySpan<byte> tail = data.Slice(blockCount * 4);
        uint tailValue = 0;
        switch (tail.Length)
        {
            case 3: tailValue ^= (uint)tail[2] << 16; goto case 2;
            case 2: tailValue ^= (uint)tail[1] << 8; goto case 1;
            case 1:
                tailValue ^= tail[0];
                tailValue *= C1;
                tailValue = RotateLeft(tailValue, 15);
                tailValue *= C2;
                hash ^= tailValue;
                break;
        }

        hash ^= (uint)length;
        hash = FinalMix(hash);
        return hash;
    }

    /// <summary>
    /// Maps a flag key + evaluation context key (+ salt) to a stable basis-point
    /// bucket in [0, 10000) — deterministic across nodes with no shared state
    /// (research R1). Format mirrors "{flagKey}:{salt}:{contextKey}" without
    /// intermediate string allocation: each segment is hashed via a stack-allocated
    /// UTF-8 buffer for short keys, falling back to a pooled buffer otherwise.
    /// </summary>
    public static int ComputeBucket(ReadOnlySpan<char> flagKey, ReadOnlySpan<char> salt, ReadOnlySpan<char> contextKey)
    {
        int totalChars = flagKey.Length + salt.Length + contextKey.Length + 2; // 2 ':' separators
        int maxBytes = totalChars * 4; // worst-case UTF-8 expansion

        Span<byte> buffer = maxBytes <= 512 ? stackalloc byte[maxBytes] : new byte[maxBytes];
        int offset = 0;

        offset += System.Text.Encoding.UTF8.GetBytes(flagKey, buffer[offset..]);
        buffer[offset++] = (byte)':';
        offset += System.Text.Encoding.UTF8.GetBytes(salt, buffer[offset..]);
        buffer[offset++] = (byte)':';
        offset += System.Text.Encoding.UTF8.GetBytes(contextKey, buffer[offset..]);

        uint hash = Hash32(buffer[..offset]);
        return (int)(hash % 10_000);
    }

    private static uint MixBody(uint hash, uint k)
    {
        k *= C1;
        k = RotateLeft(k, 15);
        k *= C2;

        hash ^= k;
        hash = RotateLeft(hash, 13);
        hash = hash * 5 + 0xe6546b64;
        return hash;
    }

    private static uint FinalMix(uint h)
    {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return h;
    }

    private static uint RotateLeft(uint x, int r) => (x << r) | (x >> (32 - r));

    private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> span) =>
        (uint)(span[0] | (span[1] << 8) | (span[2] << 16) | (span[3] << 24));
}
