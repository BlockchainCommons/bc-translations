using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Helper class for SSH key operations using BouncyCastle.
/// Handles key generation, OpenSSH format conversion, signing, and verification.
/// </summary>
internal static class SshKeyHelper
{
    /// <summary>SSH algorithm types for key generation.</summary>
    internal enum SshAlgorithm
    {
        Ed25519,
        Dsa,
        EcdsaP256,
        EcdsaP384,
    }

    /// <summary>
    /// The magic preamble used in the sshsig protocol.
    /// </summary>
    private static readonly byte[] SshSigMagicPreamble = "SSHSIG"u8.ToArray();

    /// <summary>
    /// Generates an SSH signing private key for the given algorithm.
    /// </summary>
    /// <param name="algorithm">The SSH algorithm type.</param>
    /// <param name="comment">An optional comment for the key.</param>
    /// <returns>A new <see cref="SigningPrivateKey"/> wrapping the SSH key.</returns>
    internal static SigningPrivateKey GenerateSshSigningPrivateKey(SshAlgorithm algorithm, string comment)
    {
        var rng = new SecureRandom();
        AsymmetricCipherKeyPair keyPair;

        switch (algorithm)
        {
            case SshAlgorithm.Ed25519:
            {
                var keyGen = new Ed25519KeyPairGenerator();
                keyGen.Init(new Ed25519KeyGenerationParameters(rng));
                keyPair = keyGen.GenerateKeyPair();
                break;
            }
            case SshAlgorithm.Dsa:
            {
                var keyGen = new DsaKeyPairGenerator();
                var paramGen = new DsaParametersGenerator();
                paramGen.Init(1024, 80, rng);
                var dsaParams = paramGen.GenerateParameters();
                keyGen.Init(new DsaKeyGenerationParameters(rng, dsaParams));
                keyPair = keyGen.GenerateKeyPair();
                break;
            }
            case SshAlgorithm.EcdsaP256:
            {
                var keyGen = new ECKeyPairGenerator();
                keyGen.Init(new ECKeyGenerationParameters(
                    Org.BouncyCastle.Asn1.X9.ECNamedCurveTable.GetOid("P-256"), rng));
                keyPair = keyGen.GenerateKeyPair();
                break;
            }
            case SshAlgorithm.EcdsaP384:
            {
                var keyGen = new ECKeyPairGenerator();
                keyGen.Init(new ECKeyGenerationParameters(
                    Org.BouncyCastle.Asn1.X9.ECNamedCurveTable.GetOid("P-384"), rng));
                keyPair = keyGen.GenerateKeyPair();
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(algorithm));
        }

        var pemBytes = OpenSshPrivateKeyUtilities.EncodePrivateKey(keyPair.Private);

        // Construct proper OpenSSH PEM format
        var opensshPem = FormatOpenSshPrivateKey(pemBytes);

        return SigningPrivateKey.NewSsh(opensshPem);
    }

    /// <summary>
    /// Formats raw private key bytes into OpenSSH PEM format.
    /// </summary>
    private static string FormatOpenSshPrivateKey(byte[] keyData)
    {
        var base64 = Convert.ToBase64String(keyData);
        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN OPENSSH PRIVATE KEY-----");
        for (var i = 0; i < base64.Length; i += 70)
        {
            var len = Math.Min(70, base64.Length - i);
            sb.AppendLine(base64.Substring(i, len));
        }
        sb.AppendLine("-----END OPENSSH PRIVATE KEY-----");
        return sb.ToString();
    }

    /// <summary>
    /// Derives a <see cref="SigningPublicKey"/> from an SSH private key PEM string.
    /// </summary>
    internal static SigningPublicKey DerivePublicKey(string sshPrivateKeyPem)
    {
        var privateKey = ParseOpenSshPrivateKey(sshPrivateKeyPem);
        var publicKey = GetPublicKeyFromPrivate(privateKey);
        var publicKeyBytes = OpenSshPublicKeyUtilities.EncodePublicKey(publicKey);
        var algorithmName = GetSshAlgorithmName(publicKey);
        var opensshPublicKeyText = $"{algorithmName} {Convert.ToBase64String(publicKeyBytes)}";
        return SigningPublicKey.FromSsh(opensshPublicKeyText);
    }

    /// <summary>
    /// Signs a message using the sshsig protocol.
    /// </summary>
    /// <param name="sshPrivateKeyPem">The OpenSSH private key in PEM format.</param>
    /// <param name="message">The message to sign.</param>
    /// <param name="ns">The namespace string.</param>
    /// <param name="hashAlgorithm">The hash algorithm name ("sha256" or "sha512").</param>
    /// <returns>The sshsig PEM string.</returns>
    internal static string SshSign(string sshPrivateKeyPem, byte[] message, string ns, string hashAlgorithm)
    {
        var privateKey = ParseOpenSshPrivateKey(sshPrivateKeyPem);
        var publicKey = GetPublicKeyFromPrivate(privateKey);

        // Hash the message
        var messageHash = HashMessage(message, hashAlgorithm);

        // Build the signed data: MAGIC_PREAMBLE || namespace || reserved || hash_algorithm || H(message)
        var signedData = BuildSshSigData(ns, hashAlgorithm, messageHash);

        // Sign
        var signer = CreateBcSigner(privateKey);
        signer.Init(true, privateKey);
        signer.BlockUpdate(signedData, 0, signedData.Length);
        var signatureBytes = signer.GenerateSignature();

        // Build sshsig envelope
        var publicKeyBlob = OpenSshPublicKeyUtilities.EncodePublicKey(publicKey);
        var algorithmName = GetSshAlgorithmName(publicKey);
        var sshSigBlob = BuildSshSigBlob(publicKeyBlob, algorithmName, ns, hashAlgorithm, signatureBytes, publicKey);

        var base64 = Convert.ToBase64String(sshSigBlob);
        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN SSH SIGNATURE-----");
        for (var i = 0; i < base64.Length; i += 70)
        {
            var len = Math.Min(70, base64.Length - i);
            sb.AppendLine(base64.Substring(i, len));
        }
        sb.AppendLine("-----END SSH SIGNATURE-----");
        return sb.ToString();
    }

    /// <summary>
    /// Verifies an SSH signature against a message.
    /// </summary>
    /// <param name="sshPublicKeyText">The OpenSSH public key text.</param>
    /// <param name="sshSigPem">The sshsig PEM string.</param>
    /// <param name="message">The message that was allegedly signed.</param>
    /// <returns><c>true</c> if the signature is valid.</returns>
    internal static bool SshVerify(string sshPublicKeyText, string sshSigPem, byte[] message)
    {
        try
        {
            // Parse the public key
            var parts = sshPublicKeyText.Split(' ');
            if (parts.Length < 2) return false;
            var publicKeyBlob = Convert.FromBase64String(parts[1]);
            var publicKey = OpenSshPublicKeyUtilities.ParsePublicKey(publicKeyBlob);

            // Parse the sshsig blob
            var sigBase64 = ExtractBase64FromPem(sshSigPem, "SSH SIGNATURE");
            var sigBlob = Convert.FromBase64String(sigBase64);

            // Parse sshsig blob fields
            var (ns, hashAlgorithm, signatureBytes) = ParseSshSigBlob(sigBlob, publicKey);

            // Hash the message
            var messageHash = HashMessage(message, hashAlgorithm);

            // Build the signed data
            var signedData = BuildSshSigData(ns, hashAlgorithm, messageHash);

            // Verify
            var verifier = CreateBcSigner(publicKey);
            verifier.Init(false, publicKey);
            verifier.BlockUpdate(signedData, 0, signedData.Length);
            return verifier.VerifySignature(signatureBytes);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines the <see cref="SignatureScheme"/> from an sshsig PEM string.
    /// </summary>
    internal static SignatureScheme SchemeFromSshSigPem(string sshSigPem)
    {
        try
        {
            var sigBase64 = ExtractBase64FromPem(sshSigPem, "SSH SIGNATURE");
            var sigBlob = Convert.FromBase64String(sigBase64);

            using var reader = new BinaryReader(new MemoryStream(sigBlob));

            // Read and verify magic preamble
            var magic = reader.ReadBytes(6);
            if (!magic.SequenceEqual(SshSigMagicPreamble))
                throw BCComponentsException.Ssh("Invalid sshsig magic");

            // Read version
            var version = ReadUInt32BE(reader);
            if (version != 1)
                throw BCComponentsException.Ssh("Unsupported sshsig version");

            // Read public key blob
            var pubKeyBlob = ReadString(reader);
            var publicKey = OpenSshPublicKeyUtilities.ParsePublicKey(pubKeyBlob);

            return publicKey switch
            {
                Ed25519PublicKeyParameters => SignatureScheme.SshEd25519,
                DsaPublicKeyParameters => SignatureScheme.SshDsa,
                Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ecPub =>
                    GetEcCurveScheme(ecPub),
                _ => throw BCComponentsException.Ssh("Unsupported SSH key type"),
            };
        }
        catch (BCComponentsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw BCComponentsException.Ssh($"Failed to determine scheme: {ex.Message}");
        }
    }

    // --- Private helpers ---

    private static AsymmetricKeyParameter ParseOpenSshPrivateKey(string pem)
    {
        var base64 = ExtractBase64FromPem(pem, "OPENSSH PRIVATE KEY");
        var keyData = Convert.FromBase64String(base64);
        return OpenSshPrivateKeyUtilities.ParsePrivateKeyBlob(keyData);
    }

    private static AsymmetricKeyParameter GetPublicKeyFromPrivate(AsymmetricKeyParameter privateKey)
    {
        return privateKey switch
        {
            Ed25519PrivateKeyParameters ed25519 => ed25519.GeneratePublicKey(),
            DsaPrivateKeyParameters dsa => new DsaPublicKeyParameters(
                dsa.Parameters.G.ModPow(dsa.X, dsa.Parameters.P), dsa.Parameters),
            Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters ec =>
                new Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(
                    ec.Parameters.G.Multiply(ec.D), ec.Parameters),
            _ => throw BCComponentsException.Ssh("Unsupported key type for public key derivation"),
        };
    }

    private static string GetSshAlgorithmName(AsymmetricKeyParameter key)
    {
        return key switch
        {
            Ed25519PublicKeyParameters => "ssh-ed25519",
            DsaPublicKeyParameters => "ssh-dss",
            Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ecPub =>
                GetEcCurveName(ecPub),
            _ => throw BCComponentsException.Ssh("Unsupported key type"),
        };
    }

    private static string GetEcCurveName(Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ecPub)
    {
        var bitSize = ecPub.Parameters.Curve.FieldSize;
        return bitSize switch
        {
            256 => "ecdsa-sha2-nistp256",
            384 => "ecdsa-sha2-nistp384",
            _ => throw BCComponentsException.Ssh($"Unsupported EC curve with bit size {bitSize}"),
        };
    }

    private static SignatureScheme GetEcCurveScheme(Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ecPub)
    {
        var bitSize = ecPub.Parameters.Curve.FieldSize;
        return bitSize switch
        {
            256 => SignatureScheme.SshEcdsaP256,
            384 => SignatureScheme.SshEcdsaP384,
            _ => throw BCComponentsException.Ssh($"Unsupported EC curve with bit size {bitSize}"),
        };
    }

    private static Org.BouncyCastle.Crypto.ISigner CreateBcSigner(AsymmetricKeyParameter key)
    {
        return key switch
        {
            Ed25519PrivateKeyParameters or Ed25519PublicKeyParameters =>
                new Ed25519Signer(),
            DsaPrivateKeyParameters or DsaPublicKeyParameters =>
                new DsaDigestSigner(new DsaSigner(), new Org.BouncyCastle.Crypto.Digests.Sha1Digest()),
            Org.BouncyCastle.Crypto.Parameters.ECPrivateKeyParameters ecPriv =>
                CreateEcDsaDigestSigner(ecPriv.Parameters.Curve.FieldSize),
            Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters ecPub =>
                CreateEcDsaDigestSigner(ecPub.Parameters.Curve.FieldSize),
            _ => throw BCComponentsException.Ssh("Unsupported key type for signing"),
        };
    }

    private static Org.BouncyCastle.Crypto.ISigner CreateEcDsaDigestSigner(int fieldSize)
    {
        var digest = fieldSize switch
        {
            256 => (Org.BouncyCastle.Crypto.IDigest)new Org.BouncyCastle.Crypto.Digests.Sha256Digest(),
            384 => new Org.BouncyCastle.Crypto.Digests.Sha384Digest(),
            _ => throw BCComponentsException.Ssh($"Unsupported EC curve with field size {fieldSize}"),
        };
        return new DsaDigestSigner(new ECDsaSigner(), digest);
    }

    private static byte[] HashMessage(byte[] message, string hashAlgorithm)
    {
        return hashAlgorithm.ToLowerInvariant() switch
        {
            "sha256" => SHA256.HashData(message),
            "sha512" => SHA512.HashData(message),
            _ => throw BCComponentsException.Ssh($"Unsupported hash algorithm: {hashAlgorithm}"),
        };
    }

    private static byte[] BuildSshSigData(string ns, string hashAlgorithm, byte[] messageHash)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write(SshSigMagicPreamble);
        WriteString(writer, Encoding.UTF8.GetBytes(ns));
        WriteString(writer, Array.Empty<byte>()); // reserved
        WriteString(writer, Encoding.UTF8.GetBytes(hashAlgorithm));
        WriteString(writer, messageHash);

        return ms.ToArray();
    }

    private static byte[] BuildSshSigBlob(
        byte[] publicKeyBlob,
        string algorithmName,
        string ns,
        string hashAlgorithm,
        byte[] signatureBytes,
        AsymmetricKeyParameter publicKey)
    {
        // Build the signature blob (algorithm name + signature data)
        byte[] sigBlob;
        using (var sigMs = new MemoryStream())
        using (var sigWriter = new BinaryWriter(sigMs))
        {
            // For DSA and ECDSA, we need to encode the (r,s) pair as an SSH-format blob
            var sigAlgName = publicKey switch
            {
                Ed25519PublicKeyParameters => "ssh-ed25519",
                DsaPublicKeyParameters => "ssh-dss",
                Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters => algorithmName,
                _ => algorithmName,
            };

            WriteString(sigWriter, Encoding.UTF8.GetBytes(sigAlgName));

            if (publicKey is DsaPublicKeyParameters)
            {
                // DSA signature: fixed 40-byte format (20 bytes r + 20 bytes s)
                WriteString(sigWriter, signatureBytes);
            }
            else if (publicKey is Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters)
            {
                // ECDSA: SSH format wraps r and s as mpints
                var (r, s) = DecodeRawDsaSignature(signatureBytes);
                using var ecSigMs = new MemoryStream();
                using var ecSigWriter = new BinaryWriter(ecSigMs);
                WriteMPInt(ecSigWriter, r);
                WriteMPInt(ecSigWriter, s);
                WriteString(sigWriter, ecSigMs.ToArray());
            }
            else
            {
                WriteString(sigWriter, signatureBytes);
            }

            sigBlob = sigMs.ToArray();
        }

        // Build the full sshsig blob
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write(SshSigMagicPreamble);
        WriteUInt32BE(writer, 1); // version
        WriteString(writer, publicKeyBlob);
        WriteString(writer, Encoding.UTF8.GetBytes(ns));
        WriteString(writer, Array.Empty<byte>()); // reserved
        WriteString(writer, Encoding.UTF8.GetBytes(hashAlgorithm));
        WriteString(writer, sigBlob);

        return ms.ToArray();
    }

    private static (string Namespace, string HashAlgorithm, byte[] SignatureBytes)
        ParseSshSigBlob(byte[] blob, AsymmetricKeyParameter publicKey)
    {
        using var reader = new BinaryReader(new MemoryStream(blob));

        // Magic preamble
        var magic = reader.ReadBytes(6);
        if (!magic.SequenceEqual(SshSigMagicPreamble))
            throw BCComponentsException.Ssh("Invalid sshsig magic");

        // Version
        var version = ReadUInt32BE(reader);
        if (version != 1)
            throw BCComponentsException.Ssh("Unsupported sshsig version");

        // Public key blob (skip)
        ReadString(reader);

        // Namespace
        var ns = Encoding.UTF8.GetString(ReadString(reader));

        // Reserved
        ReadString(reader);

        // Hash algorithm
        var hashAlgorithm = Encoding.UTF8.GetString(ReadString(reader));

        // Signature blob
        var sigBlob = ReadString(reader);

        // Parse the signature blob
        using var sigReader = new BinaryReader(new MemoryStream(sigBlob));
        var sigAlgName = Encoding.UTF8.GetString(ReadString(sigReader)); // algorithm name
        var rawSigData = ReadString(sigReader); // signature data

        byte[] signatureBytes;
        if (publicKey is Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters)
        {
            // Parse SSH ECDSA format (mpint r, mpint s) back to raw (r || s)
            using var ecReader = new BinaryReader(new MemoryStream(rawSigData));
            var r = ReadMPInt(ecReader);
            var s = ReadMPInt(ecReader);
            signatureBytes = EncodeRawDsaSignature(r, s);
        }
        else
        {
            signatureBytes = rawSigData;
        }

        return (ns, hashAlgorithm, signatureBytes);
    }

    private static string ExtractBase64FromPem(string pem, string label)
    {
        var beginMarker = $"-----BEGIN {label}-----";
        var endMarker = $"-----END {label}-----";
        var beginIdx = pem.IndexOf(beginMarker, StringComparison.Ordinal);
        var endIdx = pem.IndexOf(endMarker, StringComparison.Ordinal);
        if (beginIdx < 0 || endIdx < 0)
            throw BCComponentsException.Ssh($"Invalid PEM format: missing {label} markers");
        var base64Start = beginIdx + beginMarker.Length;
        var base64 = pem[base64Start..endIdx].Trim();
        // Remove all whitespace
        return base64.Replace("\n", "").Replace("\r", "").Replace(" ", "");
    }

    // --- Binary format helpers ---

    private static void WriteUInt32BE(BinaryWriter writer, uint value)
    {
        writer.Write((byte)((value >> 24) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)(value & 0xFF));
    }

    private static uint ReadUInt32BE(BinaryReader reader)
    {
        var b0 = reader.ReadByte();
        var b1 = reader.ReadByte();
        var b2 = reader.ReadByte();
        var b3 = reader.ReadByte();
        return ((uint)b0 << 24) | ((uint)b1 << 16) | ((uint)b2 << 8) | b3;
    }

    private static void WriteString(BinaryWriter writer, byte[] data)
    {
        WriteUInt32BE(writer, (uint)data.Length);
        writer.Write(data);
    }

    private static byte[] ReadString(BinaryReader reader)
    {
        var length = ReadUInt32BE(reader);
        return reader.ReadBytes((int)length);
    }

    private static void WriteMPInt(BinaryWriter writer, BigInteger value)
    {
        var bytes = value.ToByteArrayUnsigned();
        // If the high bit is set, prepend a zero byte
        if (bytes.Length > 0 && (bytes[0] & 0x80) != 0)
        {
            WriteUInt32BE(writer, (uint)(bytes.Length + 1));
            writer.Write((byte)0);
            writer.Write(bytes);
        }
        else
        {
            WriteUInt32BE(writer, (uint)bytes.Length);
            writer.Write(bytes);
        }
    }

    private static BigInteger ReadMPInt(BinaryReader reader)
    {
        var data = ReadString(reader);
        return new BigInteger(1, data);
    }

    /// <summary>
    /// Decodes a raw DSA/ECDSA signature (r || s) where each component is half the total length.
    /// For BouncyCastle DSA/ECDSA signers that return ASN.1 DER format, we need to decode differently.
    /// </summary>
    private static (BigInteger R, BigInteger S) DecodeRawDsaSignature(byte[] sig)
    {
        // BouncyCastle DSA/ECDSA signers return DER-encoded signatures
        // Parse ASN.1 SEQUENCE { INTEGER r, INTEGER s }
        var seq = (Org.BouncyCastle.Asn1.Asn1Sequence)Org.BouncyCastle.Asn1.Asn1Object.FromByteArray(sig);
        var r = ((Org.BouncyCastle.Asn1.DerInteger)seq[0]).Value;
        var s = ((Org.BouncyCastle.Asn1.DerInteger)seq[1]).Value;
        return (r, s);
    }

    private static byte[] EncodeRawDsaSignature(BigInteger r, BigInteger s)
    {
        // Encode back to DER for BouncyCastle verifier
        var seq = new Org.BouncyCastle.Asn1.DerSequence(
            new Org.BouncyCastle.Asn1.DerInteger(r),
            new Org.BouncyCastle.Asn1.DerInteger(s));
        return seq.GetDerEncoded();
    }
}
