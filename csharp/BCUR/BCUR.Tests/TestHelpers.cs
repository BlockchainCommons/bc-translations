using BlockchainCommons.BCUR;

namespace BlockchainCommons.BCUR.Tests;

/// <summary>
/// Test utilities matching the ur crate's test_utils module.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Generates a pseudo-random message of the specified size using Xoshiro256** seeded from a string.
    /// Matches ur::xoshiro::test_utils::make_message.
    /// </summary>
    internal static byte[] MakeMessage(string seed, int size)
    {
        var rng = Xoshiro256.FromString(seed);
        return rng.NextBytes(size);
    }

    /// <summary>
    /// Converts a hex string to a byte array.
    /// </summary>
    internal static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    /// <summary>
    /// Converts a byte array to a lowercase hex string.
    /// </summary>
    internal static string BytesToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
