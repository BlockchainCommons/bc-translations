using System.Runtime.InteropServices;
using System.Security.Cryptography;
using BlockchainCommons.BCCrypto;

namespace BlockchainCommons.BCShamir;

/// <summary>
/// Lagrange interpolation over GF(256) for Shamir secret sharing.
/// </summary>
internal static class Interpolation
{
    private static void ZeroUInt32(uint[] data)
    {
        CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(data.AsSpan()));
    }

    private static void HazmatLagrangeBasis(Span<byte> values, int n, ReadOnlySpan<byte> xc, byte x)
    {
        var xx = new byte[Shamir.MaxSecretLen + Shamir.MaxShareCount];
        var xSlice = new uint[8];
        var lxi = new uint[n][];
        var numerator = new uint[8];
        var denominator = new uint[8];
        var temp = new uint[8];

        xc.Slice(0, n).CopyTo(xx);

        for (var i = 0; i < n; i++)
        {
            lxi[i] = new uint[8];
            Hazmat.Bitslice(lxi[i], xx.AsSpan(i));
            xx[i + n] = xx[i];
        }

        Hazmat.BitsliceSetAll(xSlice, x);
        Hazmat.BitsliceSetAll(numerator, 1);
        Hazmat.BitsliceSetAll(denominator, 1);

        for (var i = 1; i < n; i++)
        {
            Array.Copy(xSlice, temp, 8);
            Hazmat.Gf256Add(temp, lxi[i]);
            var numerator2 = (uint[])numerator.Clone();
            Hazmat.Gf256Mul(numerator, numerator2, temp);

            Array.Copy(lxi[0], temp, 8);
            Hazmat.Gf256Add(temp, lxi[i]);
            var denominator2 = (uint[])denominator.Clone();
            Hazmat.Gf256Mul(denominator, denominator2, temp);
        }

        Hazmat.Gf256Inv(temp, denominator);

        var numeratorCopy = (uint[])numerator.Clone();
        Hazmat.Gf256Mul(numerator, numeratorCopy, temp);

        Hazmat.Unbitslice(xx, numerator);
        xx.AsSpan(0, n).CopyTo(values);

        ZeroUInt32(xSlice);
        ZeroUInt32(numerator);
        ZeroUInt32(denominator);
        ZeroUInt32(temp);
        for (var i = 0; i < lxi.Length; i++)
        {
            if (lxi[i] is not null)
                ZeroUInt32(lxi[i]);
        }
        Memzero.Zero(xx);
    }

    internal static byte[] Interpolate(
        int n,
        ReadOnlySpan<byte> xi,
        int yLength,
        IReadOnlyList<byte[]> yij,
        byte x)
    {
        if (n <= 0)
            throw new BCShamirException(ShamirError.InvalidThreshold);
        if (n > Shamir.MaxShareCount)
            throw new BCShamirException(ShamirError.TooManyShares);
        if (yLength < 0 || yLength > Shamir.MaxSecretLen)
            throw new BCShamirException(ShamirError.InterpolationFailure);
        if (xi.Length < n)
            throw new BCShamirException(ShamirError.InterpolationFailure);
        if (yij.Count < n)
            throw new BCShamirException(ShamirError.InterpolationFailure);

        var y = new byte[n][];
        for (var i = 0; i < n; i++)
            y[i] = new byte[Shamir.MaxSecretLen];

        var values = new byte[Shamir.MaxSecretLen];
        var lagrange = new byte[n];
        var ySlice = new uint[8];
        var resultSlice = new uint[8];
        var temp = new uint[8];

        try
        {
            for (var i = 0; i < n; i++)
            {
                var share = yij[i];
                if (share is null || share.Length < yLength)
                    throw new BCShamirException(ShamirError.InterpolationFailure);
                share.AsSpan(0, yLength).CopyTo(y[i]);
            }

            HazmatLagrangeBasis(lagrange, n, xi, x);
            Hazmat.BitsliceSetAll(resultSlice, 0);

            for (var i = 0; i < n; i++)
            {
                Hazmat.Bitslice(ySlice, y[i]);
                Hazmat.BitsliceSetAll(temp, lagrange[i]);
                var tempCopy = (uint[])temp.Clone();
                Hazmat.Gf256Mul(temp, tempCopy, ySlice);
                Hazmat.Gf256Add(resultSlice, temp);
            }

            Hazmat.Unbitslice(values, resultSlice);
            var result = new byte[yLength];
            values.AsSpan(0, yLength).CopyTo(result);
            return result;
        }
        finally
        {
            Memzero.Zero(lagrange);
            ZeroUInt32(ySlice);
            ZeroUInt32(resultSlice);
            ZeroUInt32(temp);
            Memzero.ZeroJaggedArray(y);
            Memzero.Zero(values);
        }
    }
}
