using System.Buffers.Binary;
using System.Security.Cryptography;

namespace BlockchainCommons.BCUR;

/// <summary>
/// Xoshiro256** pseudo-random number generator, used internally for fountain codes.
/// Seeded from a SHA-256 hash of input bytes.
/// </summary>
internal sealed class Xoshiro256
{
    private ulong _s0, _s1, _s2, _s3;

    private Xoshiro256(ulong s0, ulong s1, ulong s2, ulong s3)
    {
        _s0 = s0;
        _s1 = s1;
        _s2 = s2;
        _s3 = s3;
    }

    /// <summary>
    /// Creates a Xoshiro256** from a 32-byte seed (as produced by SHA-256).
    /// The seed bytes are interpreted as 4 big-endian u64 values,
    /// matching the Rust implementation's byte reordering (BE read → LE transmute → LE read).
    /// </summary>
    internal static Xoshiro256 FromSeed(byte[] seed)
    {
        if (seed.Length != 32)
            throw new ArgumentException("seed must be 32 bytes", nameof(seed));

        // The Rust transform reads 4 BE u64s, transmutes to bytes (native LE),
        // then reads back as LE u64s — the net effect is simply reading BE u64s.
        var s0 = BinaryPrimitives.ReadUInt64BigEndian(seed.AsSpan(0, 8));
        var s1 = BinaryPrimitives.ReadUInt64BigEndian(seed.AsSpan(8, 8));
        var s2 = BinaryPrimitives.ReadUInt64BigEndian(seed.AsSpan(16, 8));
        var s3 = BinaryPrimitives.ReadUInt64BigEndian(seed.AsSpan(24, 8));

        return new Xoshiro256(s0, s1, s2, s3);
    }

    /// <summary>
    /// Creates a Xoshiro256** from a string seed (SHA-256 hashed).
    /// </summary>
    internal static Xoshiro256 FromString(string seed)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        return FromSeed(hash);
    }

    /// <summary>
    /// Creates a Xoshiro256** from arbitrary bytes (SHA-256 hashed).
    /// </summary>
    internal static Xoshiro256 FromBytes(ReadOnlySpan<byte> data)
    {
        var hash = SHA256.HashData(data);
        return FromSeed(hash);
    }

    /// <summary>
    /// Creates a Xoshiro256** seeded from the CRC32 checksum of the given bytes.
    /// </summary>
    internal static Xoshiro256 FromCrc(ReadOnlySpan<byte> data)
    {
        var crc = Crc32.Checksum(data);
        var crcBytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc);
        return FromBytes(crcBytes);
    }

    private static ulong RotateLeft(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }

    /// <summary>
    /// Returns the next random u64.
    /// </summary>
    internal ulong Next()
    {
        var result = RotateLeft(_s1 * 5, 7) * 9;

        var t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;

        _s3 = RotateLeft(_s3, 45);

        return result;
    }

    /// <summary>
    /// Returns a random double in [0, 1).
    /// </summary>
    internal double NextDouble()
    {
        return (double)Next() / ((double)ulong.MaxValue + 1.0);
    }

    /// <summary>
    /// Returns a random integer in [low, high].
    /// </summary>
    internal ulong NextInt(ulong low, ulong high)
    {
        return (ulong)(NextDouble() * (double)(high - low + 1)) + low;
    }

    /// <summary>
    /// Returns a random byte in [0, 255].
    /// </summary>
    internal byte NextByte()
    {
        return (byte)NextInt(0, 255);
    }

    /// <summary>
    /// Returns n random bytes.
    /// </summary>
    internal byte[] NextBytes(int n)
    {
        var result = new byte[n];
        for (int i = 0; i < n; i++)
        {
            result[i] = NextByte();
        }
        return result;
    }

    /// <summary>
    /// Returns a shuffled copy of the given list.
    /// </summary>
    internal List<T> Shuffled<T>(List<T> items)
    {
        var remaining = new List<T>(items);
        var shuffled = new List<T>(items.Count);
        while (remaining.Count > 0)
        {
            var index = (int)NextInt(0, (ulong)(remaining.Count - 1));
            shuffled.Add(remaining[index]);
            remaining.RemoveAt(index);
        }
        return shuffled;
    }

    /// <summary>
    /// Chooses a degree for fountain coding using weighted sampling.
    /// </summary>
    internal int ChooseDegree(int length)
    {
        var weights = new double[length];
        for (int i = 0; i < length; i++)
        {
            weights[i] = 1.0 / (i + 1);
        }
        var sampler = new WeightedSampler(weights);
        return sampler.Next(this) + 1;
    }
}
