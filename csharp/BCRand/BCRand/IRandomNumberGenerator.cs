namespace BlockchainCommons.BCRand;

/// <summary>
/// Interface for random number generators compatible with the
/// Blockchain Commons cross-platform random number generation protocol.
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>Returns a random 32-bit unsigned integer.</summary>
    uint NextUInt32();

    /// <summary>Returns a random 64-bit unsigned integer.</summary>
    ulong NextUInt64();

    /// <summary>Returns an array of random bytes of the given size.</summary>
    /// <param name="size">The number of random bytes to generate.</param>
    /// <returns>An array containing <paramref name="size"/> random bytes.</returns>
    byte[] RandomData(int size);

    /// <summary>Fills the given span with random bytes.</summary>
    void FillRandomData(Span<byte> data);
}
