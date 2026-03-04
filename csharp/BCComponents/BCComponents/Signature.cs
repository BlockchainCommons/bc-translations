using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A digital signature created with various signature algorithms.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Signature"/> represents different types of digital signatures:
/// </para>
/// <list type="bullet">
/// <item>Schnorr: A BIP-340 Schnorr signature (64 bytes)</item>
/// <item>ECDSA: An ECDSA signature using the secp256k1 curve (64 bytes)</item>
/// <item>Ed25519: An Ed25519 signature (64 bytes)</item>
/// <item>SSH: An SSH signature in sshsig PEM format</item>
/// <item>MLDSA: A post-quantum ML-DSA signature</item>
/// </list>
/// <para>
/// Signatures can be serialized to and from CBOR with the tag 40020.
/// </para>
/// </remarks>
public sealed class Signature : IEquatable<Signature>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>The size of Schnorr, ECDSA, and Ed25519 signatures in bytes.</summary>
    public const int StandardSignatureSize = 64;

    /// <summary>Internal discriminator for signature variants.</summary>
    internal enum SignatureVariant
    {
        Schnorr,
        Ecdsa,
        Ed25519,
        Ssh,
        Mldsa,
    }

    internal SignatureVariant Variant { get; }
    private readonly byte[]? _data;         // 64 bytes for Schnorr/ECDSA/Ed25519
    private readonly string? _sshSigPem;    // PEM text for SSH
    private readonly MLDSASignature? _mldsaSig;

    private Signature(SignatureVariant variant, byte[]? data, string? sshSigPem, MLDSASignature? mldsaSig)
    {
        Variant = variant;
        _data = data;
        _sshSigPem = sshSigPem;
        _mldsaSig = mldsaSig;
    }

    // --- Factory methods ---

    /// <summary>Creates a Schnorr signature from a 64-byte array.</summary>
    /// <param name="data">The 64-byte signature data.</param>
    /// <returns>A new Schnorr <see cref="Signature"/>.</returns>
    public static Signature SchnorrFromData(byte[] data)
    {
        if (data.Length != StandardSignatureSize)
            throw BCComponentsException.InvalidSize("Schnorr signature", StandardSignatureSize, data.Length);
        return new Signature(SignatureVariant.Schnorr, (byte[])data.Clone(), null, null);
    }

    /// <summary>Creates an ECDSA signature from a 64-byte array.</summary>
    /// <param name="data">The 64-byte signature data.</param>
    /// <returns>A new ECDSA <see cref="Signature"/>.</returns>
    public static Signature EcdsaFromData(byte[] data)
    {
        if (data.Length != StandardSignatureSize)
            throw BCComponentsException.InvalidSize("ECDSA signature", StandardSignatureSize, data.Length);
        return new Signature(SignatureVariant.Ecdsa, (byte[])data.Clone(), null, null);
    }

    /// <summary>Creates an Ed25519 signature from a 64-byte array.</summary>
    /// <param name="data">The 64-byte signature data.</param>
    /// <returns>A new Ed25519 <see cref="Signature"/>.</returns>
    public static Signature Ed25519FromData(byte[] data)
    {
        if (data.Length != StandardSignatureSize)
            throw BCComponentsException.InvalidSize("Ed25519 signature", StandardSignatureSize, data.Length);
        return new Signature(SignatureVariant.Ed25519, (byte[])data.Clone(), null, null);
    }

    /// <summary>Creates an SSH signature from a PEM-encoded sshsig string.</summary>
    /// <param name="sshSigPem">The PEM-encoded SSH signature.</param>
    /// <returns>A new SSH <see cref="Signature"/>.</returns>
    public static Signature FromSshPem(string sshSigPem)
    {
        return new Signature(SignatureVariant.Ssh, null, sshSigPem, null);
    }

    /// <summary>Creates a Signature wrapping an ML-DSA signature.</summary>
    /// <param name="sig">The ML-DSA signature.</param>
    /// <returns>A new ML-DSA <see cref="Signature"/>.</returns>
    public static Signature FromMldsa(MLDSASignature sig)
    {
        return new Signature(SignatureVariant.Mldsa, null, null, sig);
    }

    // --- Accessors ---

    /// <summary>Returns the Schnorr signature data if this is a Schnorr signature, or <c>null</c>.</summary>
    public byte[]? ToSchnorr() => Variant == SignatureVariant.Schnorr ? (byte[])_data!.Clone() : null;

    /// <summary>Returns the ECDSA signature data if this is an ECDSA signature, or <c>null</c>.</summary>
    public byte[]? ToEcdsa() => Variant == SignatureVariant.Ecdsa ? (byte[])_data!.Clone() : null;

    /// <summary>Returns the Ed25519 signature data if this is an Ed25519 signature, or <c>null</c>.</summary>
    public byte[]? ToEd25519() => Variant == SignatureVariant.Ed25519 ? (byte[])_data!.Clone() : null;

    /// <summary>Returns the SSH signature PEM text if this is an SSH signature, or <c>null</c>.</summary>
    public string? ToSshPem() => Variant == SignatureVariant.Ssh ? _sshSigPem : null;

    /// <summary>Returns the ML-DSA signature if this is an ML-DSA signature, or <c>null</c>.</summary>
    public MLDSASignature? ToMldsa() => Variant == SignatureVariant.Mldsa ? _mldsaSig : null;

    /// <summary>
    /// Returns the raw signature data for Schnorr/ECDSA/Ed25519 signatures as a read-only span.
    /// </summary>
    /// <returns>The raw bytes, or an empty span for SSH/MLDSA signatures.</returns>
    internal ReadOnlySpan<byte> DataSpan() => _data ?? ReadOnlySpan<byte>.Empty;

    /// <summary>
    /// Determines the signature scheme used to create this signature.
    /// </summary>
    /// <returns>The <see cref="SignatureScheme"/> of this signature.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the scheme cannot be determined (e.g., for unsupported SSH algorithms).
    /// </exception>
    public SignatureScheme Scheme()
    {
        return Variant switch
        {
            SignatureVariant.Schnorr => SignatureScheme.Schnorr,
            SignatureVariant.Ecdsa => SignatureScheme.Ecdsa,
            SignatureVariant.Ed25519 => SignatureScheme.Ed25519,
            SignatureVariant.Ssh => SshKeyHelper.SchemeFromSshSigPem(_sshSigPem!),
            SignatureVariant.Mldsa => _mldsaSig!.Level switch
            {
                MLDSALevel.MLDSA44 => SignatureScheme.MLDSA44,
                MLDSALevel.MLDSA65 => SignatureScheme.MLDSA65,
                MLDSALevel.MLDSA87 => SignatureScheme.MLDSA87,
                _ => throw BCComponentsException.General("Unknown ML-DSA level"),
            },
            _ => throw BCComponentsException.General("Unknown signature variant"),
        };
    }

    // --- IEquatable<Signature> ---

    /// <inheritdoc/>
    public bool Equals(Signature? other)
    {
        if (other is null) return false;
        if (Variant != other.Variant) return false;
        return Variant switch
        {
            SignatureVariant.Schnorr or SignatureVariant.Ecdsa or SignatureVariant.Ed25519 =>
                _data.AsSpan().SequenceEqual(other._data),
            SignatureVariant.Ssh =>
                string.Equals(_sshSigPem, other._sshSigPem, StringComparison.Ordinal),
            SignatureVariant.Mldsa =>
                _mldsaSig!.Equals(other._mldsaSig),
            _ => false,
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Signature s && Equals(s);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Variant switch
        {
            SignatureVariant.Schnorr or SignatureVariant.Ecdsa or SignatureVariant.Ed25519 =>
                ComputeByteArrayHash(Variant, _data!),
            SignatureVariant.Ssh =>
                HashCode.Combine(Variant, _sshSigPem),
            SignatureVariant.Mldsa =>
                HashCode.Combine(Variant, _mldsaSig),
            _ => 0,
        };
    }

    private static int ComputeByteArrayHash(SignatureVariant variant, byte[] data)
    {
        var hash = new HashCode();
        hash.Add(variant);
        foreach (var b in data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Signature instances.</summary>
    public static bool operator ==(Signature? left, Signature? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Signature instances.</summary>
    public static bool operator !=(Signature? left, Signature? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40020).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagSignature);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The CBOR encoding depends on the signature type:
    /// <list type="bullet">
    /// <item>Schnorr: A byte string containing the 64-byte signature</item>
    /// <item>ECDSA: An array [1, byte_string(64)]</item>
    /// <item>Ed25519: An array [2, byte_string(64)]</item>
    /// <item>SSH: A tagged text string (tag 40802) containing the PEM-encoded signature</item>
    /// <item>ML-DSA: Delegates to <see cref="MLDSASignature"/> (tagged with 40105)</item>
    /// </list>
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        return Variant switch
        {
            SignatureVariant.Schnorr =>
                Cbor.ToByteString(_data!),
            SignatureVariant.Ecdsa =>
                Cbor.FromList(new List<Cbor> { Cbor.FromInt(1), Cbor.ToByteString(_data!) }),
            SignatureVariant.Ed25519 =>
                Cbor.FromList(new List<Cbor> { Cbor.FromInt(2), Cbor.ToByteString(_data!) }),
            SignatureVariant.Ssh =>
                Cbor.ToTaggedValue(BcTags.TagSshTextSignature, Cbor.FromString(_sshSigPem!)),
            SignatureVariant.Mldsa =>
                _mldsaSig!.TaggedCbor(),
            _ => throw BCComponentsException.General("Unknown signature variant"),
        };
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="Signature"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="Signature"/>.</returns>
    public static Signature FromUntaggedCbor(Cbor cbor)
    {
        var cborCase = cbor.Case;

        switch (cborCase)
        {
            case CborCase.ByteStringCase bs:
            {
                // Schnorr signature: plain byte string
                var data = bs.Value.ToArray();
                return SchnorrFromData(data);
            }
            case CborCase.ArrayCase arr:
            {
                var elements = arr.Value;
                if (elements.Count == 2)
                {
                    var first = elements[0].Case;

                    // Legacy Schnorr: [byte_string, ...]
                    if (first is CborCase.ByteStringCase bsCase)
                    {
                        return SchnorrFromData(bsCase.Value.ToArray());
                    }

                    if (first is CborCase.UnsignedCase unsignedCase)
                    {
                        var discriminator = unsignedCase.Value;
                        var secondCase = elements[1].Case;
                        if (secondCase is CborCase.ByteStringCase sigData)
                        {
                            return discriminator switch
                            {
                                1 => EcdsaFromData(sigData.Value.ToArray()),
                                2 => Ed25519FromData(sigData.Value.ToArray()),
                                _ => throw BCComponentsException.InvalidData("Signature",
                                    $"invalid discriminator: {discriminator}"),
                            };
                        }
                    }
                }
                throw BCComponentsException.InvalidData("Signature", "invalid array format");
            }
            case CborCase.TaggedCase tc:
            {
                var tagValue = tc.Tag.Value;

                if (tagValue == BcTags.TagMldsaSignature)
                {
                    var sig = MLDSASignature.FromTaggedCbor(cbor);
                    return FromMldsa(sig);
                }

                if (tagValue == BcTags.TagSshTextSignature)
                {
                    var pem = tc.Item.TryIntoText();
                    return FromSshPem(pem);
                }

                throw BCComponentsException.InvalidData("Signature", $"unknown tag: {tagValue}");
            }
            default:
                throw BCComponentsException.InvalidData("Signature", "invalid CBOR format");
        }
    }

    /// <summary>
    /// Decodes a <see cref="Signature"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="Signature"/>.</returns>
    public static Signature FromTaggedCbor(Cbor cbor)
    {
        foreach (var tag in CborTags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException) { }
            catch (CborWrongTypeException) { }
        }
        throw new CborWrongTypeException();
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString()
    {
        return Variant switch
        {
            SignatureVariant.Schnorr => $"Signature(Schnorr, {Convert.ToHexString(_data!).ToLowerInvariant()[..16]}...)",
            SignatureVariant.Ecdsa => $"Signature(ECDSA, {Convert.ToHexString(_data!).ToLowerInvariant()[..16]}...)",
            SignatureVariant.Ed25519 => $"Signature(Ed25519, {Convert.ToHexString(_data!).ToLowerInvariant()[..16]}...)",
            SignatureVariant.Ssh => "Signature(SSH)",
            SignatureVariant.Mldsa => $"Signature(MLDSA, {_mldsaSig!.Level})",
            _ => "Signature(Unknown)",
        };
    }
}
