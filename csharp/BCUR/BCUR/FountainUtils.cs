namespace BlockchainCommons.BCUR;

/// <summary>
/// Utility functions for fountain encoding/decoding.
/// </summary>
internal static class FountainUtils
{
    /// <summary>
    /// Calculates the optimal fragment length given the data length and maximum fragment length.
    /// </summary>
    internal static int FragmentLength(int dataLength, int maxFragmentLength)
    {
        var fragmentCount = DivCeil(dataLength, maxFragmentLength);
        return DivCeil(dataLength, fragmentCount);
    }

    /// <summary>
    /// Integer division rounding towards positive infinity.
    /// </summary>
    internal static int DivCeil(int a, int b)
    {
        var d = a / b;
        var r = a % b;
        return r > 0 ? d + 1 : d;
    }

    /// <summary>
    /// Partitions data into fragments of the specified length, zero-padding the last fragment.
    /// </summary>
    internal static List<byte[]> Partition(byte[] data, int fragmentLength)
    {
        var paddingLen = (fragmentLength - (data.Length % fragmentLength)) % fragmentLength;
        var padded = new byte[data.Length + paddingLen];
        Array.Copy(data, padded, data.Length);

        var result = new List<byte[]>();
        for (int i = 0; i < padded.Length; i += fragmentLength)
        {
            var fragment = new byte[fragmentLength];
            Array.Copy(padded, i, fragment, 0, fragmentLength);
            result.Add(fragment);
        }
        return result;
    }

    /// <summary>
    /// Chooses which fragment indexes to combine for the given sequence number.
    /// For the first fragmentCount sequences, returns the single corresponding fragment.
    /// After that, uses Xoshiro256** to randomly select and combine fragments.
    /// </summary>
    internal static List<int> ChooseFragments(int sequence, int fragmentCount, uint checksum)
    {
        if (sequence <= fragmentCount)
        {
            return [sequence - 1];
        }

        var seqU32 = (uint)sequence;
        var seed = new byte[8];
        seed[0] = (byte)(seqU32 >> 24);
        seed[1] = (byte)(seqU32 >> 16);
        seed[2] = (byte)(seqU32 >> 8);
        seed[3] = (byte)seqU32;
        seed[4] = (byte)(checksum >> 24);
        seed[5] = (byte)(checksum >> 16);
        seed[6] = (byte)(checksum >> 8);
        seed[7] = (byte)checksum;

        var rng = Xoshiro256.FromBytes(seed);
        var degree = rng.ChooseDegree(fragmentCount);

        var indexes = new List<int>(fragmentCount);
        for (int i = 0; i < fragmentCount; i++)
        {
            indexes.Add(i);
        }
        var shuffled = rng.Shuffled(indexes);
        shuffled.RemoveRange(degree, shuffled.Count - degree);
        return shuffled;
    }

    /// <summary>
    /// XORs v2 into v1 in-place.
    /// </summary>
    internal static void Xor(byte[] v1, byte[] v2)
    {
        for (int i = 0; i < v1.Length; i++)
        {
            v1[i] ^= v2[i];
        }
    }
}
