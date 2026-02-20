namespace BlockchainCommons.BCRand;

/// <summary>
/// Extension methods providing random number generation utilities
/// for any <see cref="IRandomNumberGenerator"/> implementation.
/// </summary>
public static class RandomNumberGeneratorExtensions
{
    /// <summary>
    /// Returns a random value that is less than the given upper bound.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="upperBound">The upper bound for the randomly generated value. Must be non-zero.</param>
    /// <returns>A random value in the range [0, <paramref name="upperBound"/>). Every value
    /// in the range is equally likely to be returned.</returns>
    /// <remarks>
    /// Uses Lemire's "nearly divisionless" method (32-bit variant).
    /// </remarks>
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
    /// Returns a random value that is less than the given upper bound.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="upperBound">The upper bound for the randomly generated value. Must be non-zero.</param>
    /// <returns>A random value in the range [0, <paramref name="upperBound"/>). Every value
    /// in the range is equally likely to be returned.</returns>
    /// <remarks>
    /// Uses Lemire's "nearly divisionless" method (64-bit variant).
    /// </remarks>
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
    /// Returns a random value within the specified range, using the given
    /// generator as a source for randomness.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="start">The inclusive lower bound of the range.</param>
    /// <param name="end">The exclusive upper bound of the range.</param>
    /// <returns>A random value within the half-open range
    /// [<paramref name="start"/>, <paramref name="end"/>).</returns>
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
    /// </summary>
    public static IRandomNumberGenerator ThreadRng()
    {
        return SecureRandomNumberGenerator.Shared;
    }
}
