namespace BlockchainCommons.BCRand;

/// <summary>
/// Extension methods providing random number generation utilities
/// for any <see cref="IRandomNumberGenerator"/> implementation.
/// </summary>
public static class RandomNumberGeneratorExtensions
{
    /// <summary>
    /// Returns a random value less than <paramref name="upperBound"/> using
    /// Lemire's "nearly divisionless" method (32-bit variant).
    /// </summary>
    public static uint NextWithUpperBound(this IRandomNumberGenerator rng, uint upperBound)
    {
        if (upperBound == 0)
            throw new ArgumentException("Upper bound must be non-zero.", nameof(upperBound));

        uint random = (uint)(rng.NextUInt64() & 0xFFFFFFFF);
        ulong m = (ulong)random * upperBound;
        uint low = (uint)m;

        if (low < upperBound)
        {
            uint t = (0u - upperBound) % upperBound;
            while (low < t)
            {
                random = (uint)(rng.NextUInt64() & 0xFFFFFFFF);
                m = (ulong)random * upperBound;
                low = (uint)m;
            }
        }

        return (uint)(m >> 32);
    }

    /// <summary>
    /// Returns a random value less than <paramref name="upperBound"/> using
    /// Lemire's "nearly divisionless" method (64-bit variant).
    /// </summary>
    public static ulong NextWithUpperBound(this IRandomNumberGenerator rng, ulong upperBound)
    {
        if (upperBound == 0)
            throw new ArgumentException("Upper bound must be non-zero.", nameof(upperBound));

        ulong random = rng.NextUInt64();
        UInt128 m = (UInt128)random * upperBound;
        ulong low = (ulong)m;

        if (low < upperBound)
        {
            ulong t = (0UL - upperBound) % upperBound;
            while (low < t)
            {
                random = rng.NextUInt64();
                m = (UInt128)random * upperBound;
                low = (ulong)m;
            }
        }

        return (ulong)(m >> 64);
    }

    /// <summary>
    /// Returns a random value in the half-open range [<paramref name="start"/>, <paramref name="end"/>).
    /// </summary>
    public static int NextInRange(this IRandomNumberGenerator rng, int start, int end)
    {
        if (start >= end)
            throw new ArgumentException("Start must be less than end.", nameof(start));

        uint delta = (uint)(end - start);

        if (delta == uint.MaxValue)
            return (int)(uint)rng.NextUInt64();

        uint random = rng.NextWithUpperBound(delta);
        return start + (int)random;
    }

    /// <summary>
    /// Returns a random value in the closed range [<paramref name="start"/>, <paramref name="end"/>].
    /// </summary>
    public static int NextInClosedRange(this IRandomNumberGenerator rng, int start, int end)
    {
        if (start > end)
            throw new ArgumentException("Start must be less than or equal to end.", nameof(start));

        uint delta = (uint)(end - start);

        if (delta == uint.MaxValue)
            return (int)(uint)rng.NextUInt64();

        uint random = rng.NextWithUpperBound(delta + 1);
        return start + (int)random;
    }

    /// <summary>
    /// Returns an array of random bytes of the given size.
    /// Delegates to <see cref="IRandomNumberGenerator.FillRandomData"/>.
    /// </summary>
    public static byte[] RandomArray(this IRandomNumberGenerator rng, int size)
    {
        var data = new byte[size];
        rng.FillRandomData(data);
        return data;
    }

    /// <summary>Returns a random boolean value.</summary>
    public static bool RandomBool(this IRandomNumberGenerator rng)
    {
        return rng.NextUInt32() % 2 == 0;
    }

    /// <summary>Returns a random 32-bit unsigned integer.</summary>
    public static uint RandomUInt32(this IRandomNumberGenerator rng)
    {
        return rng.NextUInt32();
    }

    /// <summary>
    /// Returns a shared thread-safe <see cref="IRandomNumberGenerator"/> instance.
    /// Equivalent to Rust's <c>thread_rng()</c>.
    /// </summary>
    public static IRandomNumberGenerator ThreadRng()
    {
        return SecureRandomNumberGenerator.Shared;
    }
}
