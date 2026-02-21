using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCShamir;

/// <summary>
/// Shamir's Secret Sharing (SSS) implementation.
/// </summary>
public static class Shamir
{
    /// <summary>The minimum length of a secret in bytes (16).</summary>
    public const int MinSecretLen = 16;

    /// <summary>The maximum length of a secret in bytes (32).</summary>
    public const int MaxSecretLen = 32;

    /// <summary>The maximum number of shares that can be generated (16).</summary>
    public const int MaxShareCount = 16;

    private const byte SecretIndex = 255;
    private const byte DigestIndex = 254;

    private static byte[] CreateDigest(ReadOnlySpan<byte> randomData, ReadOnlySpan<byte> sharedSecret)
    {
        return Hash.HmacSha256(randomData, sharedSecret);
    }

    private static void ValidateParameters(int threshold, int shareCount, int secretLength)
    {
        if (shareCount > MaxShareCount)
            throw new BCShamirException(ShamirError.TooManyShares);
        if (threshold < 1 || threshold > shareCount)
            throw new BCShamirException(ShamirError.InvalidThreshold);
        if (secretLength > MaxSecretLen)
            throw new BCShamirException(ShamirError.SecretTooLong);
        if (secretLength < MinSecretLen)
            throw new BCShamirException(ShamirError.SecretTooShort);
        if ((secretLength & 1) != 0)
            throw new BCShamirException(ShamirError.SecretNotEvenLen);
    }

    /// <summary>
    /// Splits a secret into shares using the Shamir secret sharing algorithm.
    /// </summary>
    /// <param name="threshold">The minimum number of shares required to reconstruct the secret.</param>
    /// <param name="shareCount">The total number of shares to generate.</param>
    /// <param name="secret">The secret to split. Must be between <see cref="MinSecretLen"/> and <see cref="MaxSecretLen"/> bytes, with an even length.</param>
    /// <param name="randomGenerator">The random number generator used to generate random data.</param>
    /// <returns>An array of <paramref name="shareCount"/> shares, indexed 0 through <c>shareCount - 1</c>. Pass any <paramref name="threshold"/> of them (with their indices) to <see cref="RecoverSecret"/> to reconstruct the original secret.</returns>
    public static byte[][] SplitSecret(
        int threshold,
        int shareCount,
        ReadOnlySpan<byte> secret,
        IRandomNumberGenerator randomGenerator)
    {
        ArgumentNullException.ThrowIfNull(randomGenerator);
        ValidateParameters(threshold, shareCount, secret.Length);

        if (threshold == 1)
        {
            var result = new byte[shareCount][];
            for (var i = 0; i < shareCount; i++)
                result[i] = secret.ToArray();
            return result;
        }

        var x = new byte[shareCount];
        var y = new byte[shareCount][];
        var resultShares = new byte[shareCount][];
        for (var i = 0; i < shareCount; i++)
        {
            y[i] = new byte[secret.Length];
            resultShares[i] = new byte[secret.Length];
        }

        byte[]? digest = null;
        try
        {
            var n = 0;

            for (var index = 0; index < threshold - 2; index++)
            {
                randomGenerator.FillRandomData(resultShares[index]);
                x[n] = (byte)index;
                resultShares[index].CopyTo(y[n], 0);
                n += 1;
            }

            digest = new byte[secret.Length];
            randomGenerator.FillRandomData(digest.AsSpan(4));

            var digestHash = CreateDigest(digest.AsSpan(4), secret);
            digestHash.AsSpan(0, 4).CopyTo(digest);
            x[n] = DigestIndex;
            digest.CopyTo(y[n], 0);
            n += 1;

            x[n] = SecretIndex;
            secret.CopyTo(y[n]);
            n += 1;

            for (var index = threshold - 2; index < shareCount; index++)
            {
                var interpolated = Interpolation.Interpolate(
                    n,
                    x,
                    secret.Length,
                    y,
                    (byte)index);
                interpolated.CopyTo(resultShares[index], 0);
            }

            return resultShares;
        }
        finally
        {
            if (digest is not null)
                Memzero.Zero(digest);
            Memzero.Zero(x);
            Memzero.ZeroJaggedArray(y);
        }
    }

    /// <summary>
    /// Recovers the secret from the given shares using the Shamir secret sharing algorithm.
    /// </summary>
    /// <param name="indexes">The share indexes (0-based byte values) returned by <see cref="SplitSecret"/>.</param>
    /// <param name="shares">The shares matching the given <paramref name="indexes"/>.</param>
    /// <returns>The recovered secret.</returns>
    public static byte[] RecoverSecret(IReadOnlyList<byte> indexes, IReadOnlyList<byte[]> shares)
    {
        ArgumentNullException.ThrowIfNull(indexes);
        ArgumentNullException.ThrowIfNull(shares);

        var threshold = shares.Count;
        if (threshold == 0 || indexes.Count != threshold)
            throw new BCShamirException(ShamirError.InvalidThreshold);

        var firstShare = shares[0] ?? throw new ArgumentNullException(nameof(shares), "Shares cannot contain null entries.");
        var shareLength = firstShare.Length;
        ValidateParameters(threshold, threshold, shareLength);

        for (var i = 0; i < threshold; i++)
        {
            var share = shares[i] ?? throw new ArgumentNullException(nameof(shares), "Shares cannot contain null entries.");
            if (share.Length != shareLength)
                throw new BCShamirException(ShamirError.SharesUnequalLength);
        }

        if (threshold == 1)
            return (byte[])firstShare.Clone();

        var byteIndexes = new byte[threshold];
        for (var i = 0; i < threshold; i++)
            byteIndexes[i] = indexes[i];
        byte[]? digest = null;
        byte[]? secret = null;
        byte[]? verify = null;

        try
        {
            digest = Interpolation.Interpolate(
                threshold,
                byteIndexes,
                shareLength,
                shares,
                DigestIndex);

            secret = Interpolation.Interpolate(
                threshold,
                byteIndexes,
                shareLength,
                shares,
                SecretIndex);

            verify = CreateDigest(digest.AsSpan(4), secret);

            var valid = true;
            for (var i = 0; i < 4; i++)
                valid &= digest[i] == verify[i];

            if (!valid)
            {
                Memzero.Zero(secret);
                throw new BCShamirException(ShamirError.ChecksumFailure);
            }

            return secret;
        }
        finally
        {
            if (digest is not null)
                Memzero.Zero(digest);
            if (verify is not null)
                Memzero.Zero(verify);
            Memzero.Zero(byteIndexes);
        }
    }
}
