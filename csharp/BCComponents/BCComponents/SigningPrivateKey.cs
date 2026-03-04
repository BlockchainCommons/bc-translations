using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A private key used for creating digital signatures.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SigningPrivateKey"/> is a multi-algorithm wrapper representing
/// different types of signing private keys:
/// </para>
/// <list type="bullet">
/// <item>Schnorr: BIP-340 Schnorr signing key (secp256k1)</item>
/// <item>ECDSA: ECDSA signing key (secp256k1)</item>
/// <item>Ed25519: Ed25519 signing key</item>
/// <item>SSH: SSH private key in OpenSSH format</item>
/// <item>MLDSA: Post-quantum ML-DSA signing key</item>
/// </list>
/// <para>
/// This type implements <see cref="ISigner"/>, allowing it to create signatures
/// of the appropriate type. CBOR tag: 40021.
/// </para>
/// </remarks>
public sealed class SigningPrivateKey
    : IEquatable<SigningPrivateKey>,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      ISigner,
      IReferenceProvider
{
    /// <summary>Internal discriminator for key variants.</summary>
    internal enum KeyVariant
    {
        Schnorr,
        Ecdsa,
        Ed25519,
        Ssh,
        Mldsa,
    }

    internal KeyVariant Variant { get; }
    private readonly ECPrivateKey? _ecKey;
    private readonly Ed25519PrivateKey? _ed25519Key;
    private readonly string? _sshPrivateKeyPem;
    private readonly MLDSAPrivateKey? _mldsaKey;

    private SigningPrivateKey(
        KeyVariant variant,
        ECPrivateKey? ecKey,
        Ed25519PrivateKey? ed25519Key,
        string? sshPrivateKeyPem,
        MLDSAPrivateKey? mldsaKey)
    {
        Variant = variant;
        _ecKey = ecKey;
        _ed25519Key = ed25519Key;
        _sshPrivateKeyPem = sshPrivateKeyPem;
        _mldsaKey = mldsaKey;
    }

    // --- Factory constructors ---

    /// <summary>Creates a new Schnorr signing private key from an EC private key.</summary>
    /// <param name="key">The elliptic curve private key to use.</param>
    /// <returns>A new Schnorr <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey NewSchnorr(ECPrivateKey key) =>
        new(KeyVariant.Schnorr, key, null, null, null);

    /// <summary>Creates a new ECDSA signing private key from an EC private key.</summary>
    /// <param name="key">The elliptic curve private key to use.</param>
    /// <returns>A new ECDSA <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey NewEcdsa(ECPrivateKey key) =>
        new(KeyVariant.Ecdsa, key, null, null, null);

    /// <summary>Creates a new Ed25519 signing private key.</summary>
    /// <param name="key">The Ed25519 private key to use.</param>
    /// <returns>A new Ed25519 <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey NewEd25519(Ed25519PrivateKey key) =>
        new(KeyVariant.Ed25519, null, key, null, null);

    /// <summary>Creates a new SSH signing private key from OpenSSH PEM text.</summary>
    /// <param name="sshPrivateKeyPem">The OpenSSH-formatted private key.</param>
    /// <returns>A new SSH <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey NewSsh(string sshPrivateKeyPem) =>
        new(KeyVariant.Ssh, null, null, sshPrivateKeyPem, null);

    /// <summary>Creates a new ML-DSA signing private key.</summary>
    /// <param name="key">The ML-DSA private key to use.</param>
    /// <returns>A new ML-DSA <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey NewMldsa(MLDSAPrivateKey key) =>
        new(KeyVariant.Mldsa, null, null, null, key);

    // --- Variant accessors ---

    /// <summary>Returns the underlying Schnorr key, or <c>null</c> if not a Schnorr key.</summary>
    public ECPrivateKey? ToSchnorr() => Variant == KeyVariant.Schnorr ? _ecKey : null;

    /// <summary>Returns the underlying ECDSA key, or <c>null</c> if not an ECDSA key.</summary>
    public ECPrivateKey? ToEcdsa() => Variant == KeyVariant.Ecdsa ? _ecKey : null;

    /// <summary>Returns the underlying Ed25519 key, or <c>null</c> if not an Ed25519 key.</summary>
    public Ed25519PrivateKey? ToEd25519() => Variant == KeyVariant.Ed25519 ? _ed25519Key : null;

    /// <summary>Returns the underlying SSH key PEM, or <c>null</c> if not an SSH key.</summary>
    public string? ToSshPem() => Variant == KeyVariant.Ssh ? _sshPrivateKeyPem : null;

    /// <summary>Returns the underlying ML-DSA key, or <c>null</c> if not an ML-DSA key.</summary>
    public MLDSAPrivateKey? ToMldsa() => Variant == KeyVariant.Mldsa ? _mldsaKey : null;

    /// <summary>Returns <c>true</c> if this is a Schnorr signing key.</summary>
    public bool IsSchnorr => Variant == KeyVariant.Schnorr;

    /// <summary>Returns <c>true</c> if this is an ECDSA signing key.</summary>
    public bool IsEcdsa => Variant == KeyVariant.Ecdsa;

    /// <summary>Returns <c>true</c> if this is an Ed25519 signing key.</summary>
    public bool IsEd25519 => Variant == KeyVariant.Ed25519;

    /// <summary>Returns <c>true</c> if this is an SSH signing key.</summary>
    public bool IsSsh => Variant == KeyVariant.Ssh;

    /// <summary>Returns <c>true</c> if this is an ML-DSA signing key.</summary>
    public bool IsMldsa => Variant == KeyVariant.Mldsa;

    // --- Key derivation ---

    /// <summary>
    /// Derives the corresponding <see cref="SigningPublicKey"/> from this private key.
    /// </summary>
    /// <returns>The corresponding public key.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the public key cannot be derived (e.g., for ML-DSA keys, which must
    /// be generated as a pair).
    /// </exception>
    public SigningPublicKey PublicKey()
    {
        return Variant switch
        {
            KeyVariant.Schnorr => SigningPublicKey.FromSchnorr(_ecKey!.SchnorrPublicKey()),
            KeyVariant.Ecdsa => SigningPublicKey.FromEcdsa(_ecKey!.PublicKey()),
            KeyVariant.Ed25519 => SigningPublicKey.FromEd25519(_ed25519Key!.PublicKey()),
            KeyVariant.Ssh => SshKeyHelper.DerivePublicKey(_sshPrivateKeyPem!),
            KeyVariant.Mldsa => throw BCComponentsException.General(
                "Deriving ML-DSA public key not supported; use the keypair returned by MLDSALevel.Keypair()"),
            _ => throw BCComponentsException.General("Unknown key variant"),
        };
    }

    // --- ISigner ---

    /// <summary>
    /// Signs a message with the appropriate algorithm based on the key type.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <param name="options">Optional signing options (algorithm-specific parameters).</param>
    /// <returns>The digital signature.</returns>
    /// <exception cref="BCComponentsException">Thrown if signing fails.</exception>
    public Signature SignWithOptions(byte[] message, SigningOptions? options = null)
    {
        return Variant switch
        {
            KeyVariant.Schnorr => SignSchnorr(message, options),
            KeyVariant.Ecdsa => SignEcdsa(message),
            KeyVariant.Ed25519 => SignEd25519(message),
            KeyVariant.Ssh => SignSsh(message, options),
            KeyVariant.Mldsa => SignMldsa(message),
            _ => throw BCComponentsException.General("Unknown key variant"),
        };
    }

    private Signature SignSchnorr(byte[] message, SigningOptions? options)
    {
        if (options is SigningOptions.SchnorrOptions schnorrOpts)
        {
            var sig = _ecKey!.SchnorrSignUsing(message, schnorrOpts.Rng);
            return Signature.SchnorrFromData(sig);
        }
        else
        {
            var sig = _ecKey!.SchnorrSign(message);
            return Signature.SchnorrFromData(sig);
        }
    }

    private Signature SignEcdsa(byte[] message)
    {
        var sig = _ecKey!.EcdsaSign(message);
        return Signature.EcdsaFromData(sig);
    }

    private Signature SignEd25519(byte[] message)
    {
        var sig = _ed25519Key!.Sign(message);
        return Signature.Ed25519FromData(sig);
    }

    private Signature SignSsh(byte[] message, SigningOptions? options)
    {
        if (options is not SigningOptions.SshOptions sshOpts)
            throw BCComponentsException.Ssh("Missing namespace and hash algorithm for SSH signing");
        var pem = SshKeyHelper.SshSign(_sshPrivateKeyPem!, message, sshOpts.Namespace, sshOpts.HashAlgorithm);
        return Signature.FromSshPem(pem);
    }

    private Signature SignMldsa(byte[] message)
    {
        var sig = _mldsaKey!.Sign(message);
        return Signature.FromMldsa(sig);
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<SigningPrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(SigningPrivateKey? other)
    {
        if (other is null) return false;
        if (Variant != other.Variant) return false;
        return Variant switch
        {
            KeyVariant.Schnorr => _ecKey!.Equals(other._ecKey),
            KeyVariant.Ecdsa => _ecKey!.Equals(other._ecKey),
            KeyVariant.Ed25519 => _ed25519Key!.Equals(other._ed25519Key),
            KeyVariant.Ssh => string.Equals(_sshPrivateKeyPem, other._sshPrivateKeyPem, StringComparison.Ordinal),
            KeyVariant.Mldsa => _mldsaKey!.Equals(other._mldsaKey),
            _ => false,
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SigningPrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Variant switch
        {
            KeyVariant.Schnorr => HashCode.Combine(Variant, _ecKey),
            KeyVariant.Ecdsa => HashCode.Combine(Variant, _ecKey),
            KeyVariant.Ed25519 => HashCode.Combine(Variant, _ed25519Key),
            KeyVariant.Ssh => HashCode.Combine(Variant, _sshPrivateKeyPem),
            KeyVariant.Mldsa => HashCode.Combine(Variant, _mldsaKey),
            _ => 0,
        };
    }

    /// <summary>Tests equality of two SigningPrivateKey instances.</summary>
    public static bool operator ==(SigningPrivateKey? left, SigningPrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two SigningPrivateKey instances.</summary>
    public static bool operator !=(SigningPrivateKey? left, SigningPrivateKey? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40021).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagSigningPrivateKey);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The CBOR encoding depends on the key type:
    /// <list type="bullet">
    /// <item>Schnorr: A byte string containing the 32-byte private key</item>
    /// <item>ECDSA: An array [1, byte_string(32)]</item>
    /// <item>Ed25519: An array [2, byte_string(32)]</item>
    /// <item>SSH: A tagged text string (tag 40800) containing the OpenSSH private key</item>
    /// <item>ML-DSA: Delegates to <see cref="MLDSAPrivateKey"/> (tagged with 40103)</item>
    /// </list>
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        return Variant switch
        {
            KeyVariant.Schnorr =>
                Cbor.ToByteString(_ecKey!.Data),
            KeyVariant.Ecdsa =>
                Cbor.FromList(new List<Cbor> { Cbor.FromInt(1), Cbor.ToByteString(_ecKey!.Data) }),
            KeyVariant.Ed25519 =>
                Cbor.FromList(new List<Cbor> { Cbor.FromInt(2), Cbor.ToByteString(_ed25519Key!.Data) }),
            KeyVariant.Ssh =>
                Cbor.ToTaggedValue(BcTags.TagSshTextPrivateKey, Cbor.FromString(_sshPrivateKeyPem!)),
            KeyVariant.Mldsa =>
                _mldsaKey!.TaggedCbor(),
            _ => throw BCComponentsException.General("Unknown key variant"),
        };
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="SigningPrivateKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey FromUntaggedCbor(Cbor cbor)
    {
        var cborCase = cbor.Case;

        switch (cborCase)
        {
            case CborCase.ByteStringCase bs:
            {
                // Schnorr: plain byte string
                var data = bs.Value.ToArray();
                return NewSchnorr(ECPrivateKey.FromData(data));
            }
            case CborCase.ArrayCase arr:
            {
                var elements = arr.Value;
                if (elements.Count < 2)
                    throw BCComponentsException.InvalidData("SigningPrivateKey", "array too short");
                var discriminator = (int)elements[0].TryIntoUInt64();
                var data = elements[1].TryIntoByteString();
                return discriminator switch
                {
                    1 => NewEcdsa(ECPrivateKey.FromData(data)),
                    2 => NewEd25519(Ed25519PrivateKey.FromData(data)),
                    _ => throw BCComponentsException.InvalidData("SigningPrivateKey",
                        $"invalid discriminator: {discriminator}"),
                };
            }
            case CborCase.TaggedCase tc:
            {
                var tagValue = tc.Tag.Value;

                if (tagValue == BcTags.TagSshTextPrivateKey)
                {
                    var pem = tc.Item.TryIntoText();
                    return NewSsh(pem);
                }

                if (tagValue == BcTags.TagMldsaPrivateKey)
                {
                    var key = MLDSAPrivateKey.FromTaggedCbor(cbor);
                    return NewMldsa(key);
                }

                throw BCComponentsException.InvalidData("SigningPrivateKey", $"unknown tag: {tagValue}");
            }
            default:
                throw BCComponentsException.InvalidData("SigningPrivateKey", "invalid CBOR format");
        }
    }

    /// <summary>
    /// Decodes a <see cref="SigningPrivateKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="SigningPrivateKey"/>.</returns>
    public static SigningPrivateKey FromTaggedCbor(Cbor cbor)
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
        var inner = Variant switch
        {
            KeyVariant.Schnorr => $"Schnorr({_ecKey!})",
            KeyVariant.Ecdsa => $"ECDSA({_ecKey!})",
            KeyVariant.Ed25519 => $"Ed25519({_ed25519Key!})",
            KeyVariant.Ssh => "SSH",
            KeyVariant.Mldsa => $"MLDSA({_mldsaKey!.Level})",
            _ => "Unknown",
        };
        return $"SigningPrivateKey({((IReferenceProvider)this).RefHexShort()}, {inner})";
    }
}
