using System.Buffers.Binary;
using System.Numerics;

namespace BlockchainCommons.BCRand;

/// <summary>
/// Internal implementation of the Xoshiro256** PRNG algorithm.
/// Used by <see cref="SeededRandomNumberGenerator"/> for deterministic output.
/// </summary>
internal sealed class Xoshiro256StarStar
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    /// <summary>
    /// Initializes the PRNG from a 32-byte seed (4 little-endian 64-bit unsigned integers).
    /// </summary>
    public Xoshiro256StarStar(ReadOnlySpan<byte> seed)
    {
        if (seed.Length != 32)
            throw new ArgumentException("Seed must be exactly 32 bytes.", nameof(seed));

        _s0 = BinaryPrimitives.ReadUInt64LittleEndian(seed);
        _s1 = BinaryPrimitives.ReadUInt64LittleEndian(seed[8..]);
        _s2 = BinaryPrimitives.ReadUInt64LittleEndian(seed[16..]);
        _s3 = BinaryPrimitives.ReadUInt64LittleEndian(seed[24..]);
    }

    public ulong NextUInt64()
    {
        ulong result = BitOperations.RotateLeft(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);

        return result;
    }
}
