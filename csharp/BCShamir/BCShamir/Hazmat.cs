using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BlockchainCommons.BCShamir;

/// <summary>
/// Bitsliced GF(256) arithmetic primitives for constant-time Shamir interpolation.
/// </summary>
internal static class Hazmat
{
    private const int FieldBits = 8;
    private const int BlockSize = 32;

    private static void ValidateFieldVector(uint[] vector, string paramName)
    {
        if (vector.Length != FieldBits)
            throw new ArgumentException($"Expected a {FieldBits}-element field vector.", paramName);
    }

    private static void ZeroUInt32(uint[] data)
    {
        CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(data.AsSpan()));
    }

    public static void Bitslice(uint[] r, ReadOnlySpan<byte> x)
    {
        ValidateFieldVector(r, nameof(r));
        if (x.Length < BlockSize)
            throw new ArgumentException("Input must be at least 32 bytes.", nameof(x));

        ZeroUInt32(r);
        for (var arrIdx = 0; arrIdx < BlockSize; arrIdx++)
        {
            var current = (uint)x[arrIdx];
            for (var bitIdx = 0; bitIdx < FieldBits; bitIdx++)
            {
                r[bitIdx] |= ((current & (1u << bitIdx)) >> bitIdx) << arrIdx;
            }
        }
    }

    public static void Unbitslice(Span<byte> r, uint[] x)
    {
        ValidateFieldVector(x, nameof(x));
        if (r.Length < BlockSize)
            throw new ArgumentException("Output must be at least 32 bytes.", nameof(r));

        CryptographicOperations.ZeroMemory(r);
        for (var bitIdx = 0; bitIdx < FieldBits; bitIdx++)
        {
            var current = x[bitIdx];
            for (var arrIdx = 0; arrIdx < BlockSize; arrIdx++)
            {
                r[arrIdx] |= (byte)(((current & (1u << arrIdx)) >> arrIdx) << bitIdx);
            }
        }
    }

    public static void BitsliceSetAll(uint[] r, byte x)
    {
        ValidateFieldVector(r, nameof(r));

        for (var idx = 0; idx < FieldBits; idx++)
        {
            var masked = ((uint)x & (1u << idx)) << (31 - idx);
            r[idx] = unchecked((uint)(((int)masked) >> 31));
        }
    }

    public static void Gf256Add(uint[] r, uint[] x)
    {
        ValidateFieldVector(r, nameof(r));
        ValidateFieldVector(x, nameof(x));

        for (var i = 0; i < FieldBits; i++)
            r[i] ^= x[i];
    }

    public static void Gf256Mul(uint[] r, uint[] a, uint[] b)
    {
        ValidateFieldVector(r, nameof(r));
        ValidateFieldVector(a, nameof(a));
        ValidateFieldVector(b, nameof(b));

        var a2 = (uint[])a.Clone();

        r[0] = a2[0] & b[0];
        r[1] = a2[1] & b[0];
        r[2] = a2[2] & b[0];
        r[3] = a2[3] & b[0];
        r[4] = a2[4] & b[0];
        r[5] = a2[5] & b[0];
        r[6] = a2[6] & b[0];
        r[7] = a2[7] & b[0];
        a2[0] ^= a2[7];
        a2[2] ^= a2[7];
        a2[3] ^= a2[7];

        r[0] ^= a2[7] & b[1];
        r[1] ^= a2[0] & b[1];
        r[2] ^= a2[1] & b[1];
        r[3] ^= a2[2] & b[1];
        r[4] ^= a2[3] & b[1];
        r[5] ^= a2[4] & b[1];
        r[6] ^= a2[5] & b[1];
        r[7] ^= a2[6] & b[1];
        a2[7] ^= a2[6];
        a2[1] ^= a2[6];
        a2[2] ^= a2[6];

        r[0] ^= a2[6] & b[2];
        r[1] ^= a2[7] & b[2];
        r[2] ^= a2[0] & b[2];
        r[3] ^= a2[1] & b[2];
        r[4] ^= a2[2] & b[2];
        r[5] ^= a2[3] & b[2];
        r[6] ^= a2[4] & b[2];
        r[7] ^= a2[5] & b[2];
        a2[6] ^= a2[5];
        a2[0] ^= a2[5];
        a2[1] ^= a2[5];

        r[0] ^= a2[5] & b[3];
        r[1] ^= a2[6] & b[3];
        r[2] ^= a2[7] & b[3];
        r[3] ^= a2[0] & b[3];
        r[4] ^= a2[1] & b[3];
        r[5] ^= a2[2] & b[3];
        r[6] ^= a2[3] & b[3];
        r[7] ^= a2[4] & b[3];
        a2[5] ^= a2[4];
        a2[7] ^= a2[4];
        a2[0] ^= a2[4];

        r[0] ^= a2[4] & b[4];
        r[1] ^= a2[5] & b[4];
        r[2] ^= a2[6] & b[4];
        r[3] ^= a2[7] & b[4];
        r[4] ^= a2[0] & b[4];
        r[5] ^= a2[1] & b[4];
        r[6] ^= a2[2] & b[4];
        r[7] ^= a2[3] & b[4];
        a2[4] ^= a2[3];
        a2[6] ^= a2[3];
        a2[7] ^= a2[3];

        r[0] ^= a2[3] & b[5];
        r[1] ^= a2[4] & b[5];
        r[2] ^= a2[5] & b[5];
        r[3] ^= a2[6] & b[5];
        r[4] ^= a2[7] & b[5];
        r[5] ^= a2[0] & b[5];
        r[6] ^= a2[1] & b[5];
        r[7] ^= a2[2] & b[5];
        a2[3] ^= a2[2];
        a2[5] ^= a2[2];
        a2[6] ^= a2[2];

        r[0] ^= a2[2] & b[6];
        r[1] ^= a2[3] & b[6];
        r[2] ^= a2[4] & b[6];
        r[3] ^= a2[5] & b[6];
        r[4] ^= a2[6] & b[6];
        r[5] ^= a2[7] & b[6];
        r[6] ^= a2[0] & b[6];
        r[7] ^= a2[1] & b[6];
        a2[2] ^= a2[1];
        a2[4] ^= a2[1];
        a2[5] ^= a2[1];

        r[0] ^= a2[1] & b[7];
        r[1] ^= a2[2] & b[7];
        r[2] ^= a2[3] & b[7];
        r[3] ^= a2[4] & b[7];
        r[4] ^= a2[5] & b[7];
        r[5] ^= a2[6] & b[7];
        r[6] ^= a2[7] & b[7];
        r[7] ^= a2[0] & b[7];
    }

    public static void Gf256Square(uint[] r, uint[] x)
    {
        ValidateFieldVector(r, nameof(r));
        ValidateFieldVector(x, nameof(x));

        uint r8;
        uint r10;
        var r14 = x[7];
        var r12 = x[6];
        r10 = x[5];
        r8 = x[4];
        r[6] = x[3];
        r[4] = x[2];
        r[2] = x[1];
        r[0] = x[0];

        r[7] = r14;
        r[6] ^= r14;
        r10 ^= r14;
        r[4] ^= r12;
        r[5] = r12;
        r[7] ^= r12;
        r8 ^= r12;
        r[2] ^= r10;
        r[3] = r10;
        r[5] ^= r10;
        r[6] ^= r10;
        r[1] = r14;
        r[2] ^= r14;
        r[4] ^= r14;
        r[5] ^= r14;
        r[0] ^= r8;
        r[1] ^= r8;
        r[3] ^= r8;
        r[4] ^= r8;
    }

    public static void Gf256Inv(uint[] r, uint[] x)
    {
        ValidateFieldVector(r, nameof(r));
        ValidateFieldVector(x, nameof(x));

        var y = new uint[FieldBits];
        var z = new uint[FieldBits];

        Gf256Square(y, x);
        var y2 = (uint[])y.Clone();
        Gf256Square(y, y2);
        Gf256Square(r, y);
        Gf256Mul(z, r, x);

        var r2 = (uint[])r.Clone();
        Gf256Square(r, r2);
        r2 = (uint[])r.Clone();
        Gf256Mul(r, r2, z);
        r2 = (uint[])r.Clone();
        Gf256Square(r, r2);
        Gf256Square(z, r);

        var z2 = (uint[])z.Clone();
        Gf256Square(z, z2);
        r2 = (uint[])r.Clone();
        Gf256Mul(r, r2, z);
        r2 = (uint[])r.Clone();
        Gf256Mul(r, r2, y);

        ZeroUInt32(y);
        ZeroUInt32(z);
    }
}
