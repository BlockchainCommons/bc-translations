using System.Security.Cryptography;
using System.Text;
using BlockchainCommons.BCRand;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Math;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Schnorr (BIP-340) signing and verification on secp256k1.
/// Implements the full BIP-340 algorithm with support for variable-length messages.
/// </summary>
public static class SchnorrSigning
{
    public const int SchnorrSignatureSize = 64;

    private static readonly Org.BouncyCastle.Asn1.X9.X9ECParameters Curve =
        CustomNamedCurves.GetByName("secp256k1");
    private static readonly ECPoint G = Curve.G;
    private static readonly BigInteger N = Curve.N;
    private static readonly BigInteger FieldP = new BigInteger(1,
        Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F"));

    /// <summary>Signs a message using Schnorr with secure random auxiliary randomness.</summary>
    /// <param name="ecdsaPrivateKey">The 32-byte ECDSA private key.</param>
    /// <param name="message">The message to sign (any length).</param>
    /// <returns>A 64-byte Schnorr signature.</returns>
    public static byte[] SchnorrSign(ReadOnlySpan<byte> ecdsaPrivateKey, ReadOnlySpan<byte> message)
    {
        var rng = new SecureRandomNumberGenerator();
        return SchnorrSignUsing(ecdsaPrivateKey, message, rng);
    }

    /// <summary>Signs a message using Schnorr with the given random number generator for auxiliary randomness.</summary>
    /// <param name="ecdsaPrivateKey">The 32-byte ECDSA private key.</param>
    /// <param name="message">The message to sign (any length).</param>
    /// <param name="rng">The random number generator for auxiliary randomness.</param>
    /// <returns>A 64-byte Schnorr signature.</returns>
    public static byte[] SchnorrSignUsing(
        ReadOnlySpan<byte> ecdsaPrivateKey,
        ReadOnlySpan<byte> message,
        IRandomNumberGenerator rng)
    {
        byte[] auxRand = rng.RandomData(32);
        return SchnorrSignWithAuxRand(ecdsaPrivateKey, message, auxRand);
    }

    /// <summary>Signs a message using Schnorr with the given auxiliary randomness.</summary>
    /// <param name="ecdsaPrivateKey">The 32-byte ECDSA private key.</param>
    /// <param name="message">The message to sign (any length).</param>
    /// <param name="auxRand">32 bytes of auxiliary randomness.</param>
    /// <returns>A 64-byte Schnorr signature.</returns>
    public static byte[] SchnorrSignWithAuxRand(
        ReadOnlySpan<byte> ecdsaPrivateKey,
        ReadOnlySpan<byte> message,
        ReadOnlySpan<byte> auxRand)
    {
        // BIP-340 signing algorithm
        var d0 = new BigInteger(1, ecdsaPrivateKey.ToArray());

        ECPoint pointP = G.Multiply(d0).Normalize();
        byte[] px = PointXBytes(pointP);

        BigInteger d = HasEvenY(pointP) ? d0 : N.Subtract(d0);

        byte[] dBytes = PadTo32(d.ToByteArrayUnsigned());
        byte[] auxHash = TaggedHash("BIP0340/aux", auxRand);
        byte[] t = Xor32(dBytes, auxHash);

        byte[] nonceInput = Concat(t, px, message.ToArray());
        byte[] rand = TaggedHash("BIP0340/nonce", nonceInput);

        var k0 = new BigInteger(1, rand).Mod(N);
        if (k0.SignValue == 0)
            throw new BCCryptoException("Schnorr nonce is zero");

        ECPoint pointR = G.Multiply(k0).Normalize();
        byte[] rx = PointXBytes(pointR);

        BigInteger k = HasEvenY(pointR) ? k0 : N.Subtract(k0);

        byte[] challengeInput = Concat(rx, px, message.ToArray());
        byte[] eHash = TaggedHash("BIP0340/challenge", challengeInput);
        var e = new BigInteger(1, eHash).Mod(N);

        BigInteger s = k.Add(e.Multiply(d)).Mod(N);
        byte[] sBytes = PadTo32(s.ToByteArrayUnsigned());

        byte[] sig = new byte[SchnorrSignatureSize];
        Buffer.BlockCopy(rx, 0, sig, 0, 32);
        Buffer.BlockCopy(sBytes, 0, sig, 32, 32);
        return sig;
    }

    /// <summary>Verifies a Schnorr signature using the given x-only public key.</summary>
    /// <param name="schnorrPublicKey">The 32-byte x-only Schnorr public key.</param>
    /// <param name="schnorrSignature">The 64-byte Schnorr signature.</param>
    /// <param name="message">The original message (any length).</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown if the public key is invalid.</exception>
    public static bool SchnorrVerify(
        ReadOnlySpan<byte> schnorrPublicKey,
        ReadOnlySpan<byte> schnorrSignature,
        ReadOnlySpan<byte> message)
    {
        if (schnorrPublicKey.Length != 32 || schnorrSignature.Length != 64)
            return false;

        // Decode x-only public key (even y) via SEC 1 compressed encoding
        byte[] encoded = new byte[33];
        encoded[0] = 0x02; // even y prefix
        schnorrPublicKey.CopyTo(encoded.AsSpan(1));
        ECPoint pointP;
        try
        {
            pointP = Curve.Curve.DecodePoint(encoded).Normalize();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid Schnorr public key", ex);
        }

        var r = new BigInteger(1, schnorrSignature[..32].ToArray());
        var s = new BigInteger(1, schnorrSignature[32..].ToArray());

        if (r.CompareTo(FieldP) >= 0 || s.CompareTo(N) >= 0)
            return false;

        byte[] rx = PadTo32(r.ToByteArrayUnsigned());
        byte[] px = PointXBytes(pointP);
        byte[] challengeInput = Concat(rx, px, message.ToArray());
        byte[] eHash = TaggedHash("BIP0340/challenge", challengeInput);
        var e = new BigInteger(1, eHash).Mod(N);

        // R = s·G - e·P
        ECPoint pointR = G.Multiply(s).Add(pointP.Multiply(N.Subtract(e))).Normalize();

        if (pointR.IsInfinity)
            return false;
        if (!HasEvenY(pointR))
            return false;
        if (!pointR.AffineXCoord.ToBigInteger().Equals(r))
            return false;

        return true;
    }

    private static byte[] TaggedHash(string tag, ReadOnlySpan<byte> data)
    {
        byte[] tagHash = SHA256.HashData(Encoding.UTF8.GetBytes(tag));
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha.AppendData(tagHash);
        sha.AppendData(tagHash);
        sha.AppendData(data);
        return sha.GetHashAndReset();
    }

    private static bool HasEvenY(ECPoint point)
    {
        return !point.Normalize().AffineYCoord.ToBigInteger().TestBit(0);
    }

    private static byte[] PointXBytes(ECPoint point)
    {
        return PadTo32(point.Normalize().AffineXCoord.GetEncoded());
    }

    private static byte[] PadTo32(byte[] data)
    {
        if (data.Length == 32) return data;
        if (data.Length > 32)
        {
            byte[] result = new byte[32];
            Buffer.BlockCopy(data, data.Length - 32, result, 0, 32);
            return result;
        }
        byte[] padded = new byte[32];
        Buffer.BlockCopy(data, 0, padded, 32 - data.Length, data.Length);
        return padded;
    }

    private static byte[] Xor32(byte[] a, byte[] b)
    {
        byte[] result = new byte[32];
        for (int i = 0; i < 32; i++)
            result[i] = (byte)(a[i] ^ b[i]);
        return result;
    }

    private static byte[] Concat(params byte[][] arrays)
    {
        int totalLength = 0;
        foreach (var a in arrays) totalLength += a.Length;
        byte[] result = new byte[totalLength];
        int offset = 0;
        foreach (var a in arrays)
        {
            Buffer.BlockCopy(a, 0, result, offset, a.Length);
            offset += a.Length;
        }
        return result;
    }
}
