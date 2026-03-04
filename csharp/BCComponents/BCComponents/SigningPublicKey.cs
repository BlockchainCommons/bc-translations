using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A public key used for verifying digital signatures.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SigningPublicKey"/> is a multi-algorithm wrapper representing
/// different types of signing public keys:
/// </para>
/// <list type="bullet">
/// <item>Schnorr: BIP-340 x-only public key (32 bytes)</item>
/// <item>ECDSA: Compressed secp256k1 public key (33 bytes)</item>
/// <item>Ed25519: Ed25519 public key (32 bytes)</item>
/// <item>SSH: SSH public key in OpenSSH format</item>
/// <item>MLDSA: Post-quantum ML-DSA public key</item>
/// </list>
/// <para>
/// This type implements <see cref="IVerifier"/>, allowing it to verify signatures
/// of the appropriate type. CBOR tag: 40022.
/// </para>
/// </remarks>
public sealed class SigningPublicKey
    : IEquatable<SigningPublicKey>,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      IVerifier,
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
    private readonly SchnorrPublicKey? _schnorrKey;
    private readonly ECPublicKey? _ecdsaKey;
    private readonly Ed25519PublicKey? _ed25519Key;
    private readonly string? _sshPublicKeyText;
    private readonly MLDSAPublicKey? _mldsaKey;

    private SigningPublicKey(
        KeyVariant variant,
        SchnorrPublicKey? schnorrKey,
        ECPublicKey? ecdsaKey,
        Ed25519PublicKey? ed25519Key,
        string? sshPublicKeyText,
        MLDSAPublicKey? mldsaKey)
    {
        Variant = variant;
        _schnorrKey = schnorrKey;
        _ecdsaKey = ecdsaKey;
        _ed25519Key = ed25519Key;
        _sshPublicKeyText = sshPublicKeyText;
        _mldsaKey = mldsaKey;
    }

    // --- Factory constructors ---

    /// <summary>Creates a new signing public key from a Schnorr public key.</summary>
    /// <param name="key">A BIP-340 Schnorr (x-only) public key.</param>
    /// <returns>A new Schnorr <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromSchnorr(SchnorrPublicKey key) =>
        new(KeyVariant.Schnorr, key, null, null, null, null);

    /// <summary>Creates a new signing public key from an ECDSA public key.</summary>
    /// <param name="key">A compressed ECDSA public key.</param>
    /// <returns>A new ECDSA <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromEcdsa(ECPublicKey key) =>
        new(KeyVariant.Ecdsa, null, key, null, null, null);

    /// <summary>Creates a new signing public key from an Ed25519 public key.</summary>
    /// <param name="key">An Ed25519 public key.</param>
    /// <returns>A new Ed25519 <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromEd25519(Ed25519PublicKey key) =>
        new(KeyVariant.Ed25519, null, null, key, null, null);

    /// <summary>Creates a new signing public key from an OpenSSH public key string.</summary>
    /// <param name="sshPublicKeyText">The OpenSSH-formatted public key.</param>
    /// <returns>A new SSH <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromSsh(string sshPublicKeyText) =>
        new(KeyVariant.Ssh, null, null, null, sshPublicKeyText, null);

    /// <summary>Creates a new signing public key from an ML-DSA public key.</summary>
    /// <param name="key">The ML-DSA public key.</param>
    /// <returns>A new ML-DSA <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromMldsa(MLDSAPublicKey key) =>
        new(KeyVariant.Mldsa, null, null, null, null, key);

    // --- Variant accessors ---

    /// <summary>Returns the underlying Schnorr public key, or <c>null</c>.</summary>
    public SchnorrPublicKey? ToSchnorr() => Variant == KeyVariant.Schnorr ? _schnorrKey : null;

    /// <summary>Returns the underlying ECDSA public key, or <c>null</c>.</summary>
    public ECPublicKey? ToEcdsa() => Variant == KeyVariant.Ecdsa ? _ecdsaKey : null;

    /// <summary>Returns the underlying Ed25519 public key, or <c>null</c>.</summary>
    public Ed25519PublicKey? ToEd25519() => Variant == KeyVariant.Ed25519 ? _ed25519Key : null;

    /// <summary>Returns the underlying SSH public key text, or <c>null</c>.</summary>
    public string? ToSshText() => Variant == KeyVariant.Ssh ? _sshPublicKeyText : null;

    /// <summary>Returns the underlying ML-DSA public key, or <c>null</c>.</summary>
    public MLDSAPublicKey? ToMldsa() => Variant == KeyVariant.Mldsa ? _mldsaKey : null;

    // --- IVerifier ---

    /// <summary>
    /// Verifies a signature against a message.
    /// </summary>
    /// <remarks>
    /// The type of signature must match the type of this key for verification to succeed.
    /// </remarks>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="message">The message that was allegedly signed.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid for the message; <c>false</c> otherwise.
    /// </returns>
    public bool Verify(Signature signature, byte[] message)
    {
        return Variant switch
        {
            KeyVariant.Schnorr => VerifySchnorr(signature, message),
            KeyVariant.Ecdsa => VerifyEcdsa(signature, message),
            KeyVariant.Ed25519 => VerifyEd25519(signature, message),
            KeyVariant.Ssh => VerifySsh(signature, message),
            KeyVariant.Mldsa => VerifyMldsa(signature, message),
            _ => false,
        };
    }

    private bool VerifySchnorr(Signature signature, byte[] message)
    {
        var sigData = signature.ToSchnorr();
        if (sigData is null) return false;
        return _schnorrKey!.SchnorrVerify(sigData, message);
    }

    private bool VerifyEcdsa(Signature signature, byte[] message)
    {
        var sigData = signature.ToEcdsa();
        if (sigData is null) return false;
        return _ecdsaKey!.Verify(sigData, message);
    }

    private bool VerifyEd25519(Signature signature, byte[] message)
    {
        var sigData = signature.ToEd25519();
        if (sigData is null) return false;
        return _ed25519Key!.Verify(sigData, message);
    }

    private bool VerifySsh(Signature signature, byte[] message)
    {
        var sigPem = signature.ToSshPem();
        if (sigPem is null) return false;
        return SshKeyHelper.SshVerify(_sshPublicKeyText!, sigPem, message);
    }

    private bool VerifyMldsa(Signature signature, byte[] message)
    {
        var mldsaSig = signature.ToMldsa();
        if (mldsaSig is null) return false;
        try
        {
            return _mldsaKey!.Verify(mldsaSig, message);
        }
        catch
        {
            return false;
        }
    }

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<SigningPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(SigningPublicKey? other)
    {
        if (other is null) return false;
        if (Variant != other.Variant) return false;
        return Variant switch
        {
            KeyVariant.Schnorr => _schnorrKey!.Equals(other._schnorrKey),
            KeyVariant.Ecdsa => _ecdsaKey!.Equals(other._ecdsaKey),
            KeyVariant.Ed25519 => _ed25519Key!.Equals(other._ed25519Key),
            KeyVariant.Ssh => string.Equals(_sshPublicKeyText, other._sshPublicKeyText, StringComparison.Ordinal),
            KeyVariant.Mldsa => _mldsaKey!.Equals(other._mldsaKey),
            _ => false,
        };
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SigningPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Variant switch
        {
            KeyVariant.Schnorr => HashCode.Combine(Variant, _schnorrKey),
            KeyVariant.Ecdsa => HashCode.Combine(Variant, _ecdsaKey),
            KeyVariant.Ed25519 => HashCode.Combine(Variant, _ed25519Key),
            KeyVariant.Ssh => HashCode.Combine(Variant, _sshPublicKeyText),
            KeyVariant.Mldsa => HashCode.Combine(Variant, _mldsaKey),
            _ => 0,
        };
    }

    /// <summary>Tests equality of two SigningPublicKey instances.</summary>
    public static bool operator ==(SigningPublicKey? left, SigningPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two SigningPublicKey instances.</summary>
    public static bool operator !=(SigningPublicKey? left, SigningPublicKey? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40022).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagSigningPublicKey);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The CBOR encoding depends on the key type:
    /// <list type="bullet">
    /// <item>Schnorr: A byte string containing the 32-byte x-only public key</item>
    /// <item>ECDSA: An array [1, byte_string(33)]</item>
    /// <item>Ed25519: An array [2, byte_string(32)]</item>
    /// <item>SSH: A tagged text string (tag 40801) containing the OpenSSH public key</item>
    /// <item>ML-DSA: Delegates to <see cref="MLDSAPublicKey"/> (tagged with 40104)</item>
    /// </list>
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        return Variant switch
        {
            KeyVariant.Schnorr =>
                Cbor.ToByteString(_schnorrKey!.Data),
            KeyVariant.Ecdsa =>
                Cbor.FromList(new List<Cbor> { Cbor.FromInt(1), Cbor.ToByteString(_ecdsaKey!.Data) }),
            KeyVariant.Ed25519 =>
                Cbor.FromList(new List<Cbor> { Cbor.FromInt(2), Cbor.ToByteString(_ed25519Key!.Data) }),
            KeyVariant.Ssh =>
                Cbor.ToTaggedValue(BcTags.TagSshTextPublicKey, Cbor.FromString(_sshPublicKeyText!)),
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
    /// Decodes a <see cref="SigningPublicKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromUntaggedCbor(Cbor cbor)
    {
        var cborCase = cbor.Case;

        switch (cborCase)
        {
            case CborCase.ByteStringCase bs:
            {
                // Schnorr: plain byte string (32 bytes)
                var data = bs.Value.ToArray();
                return FromSchnorr(SchnorrPublicKey.FromData(data));
            }
            case CborCase.ArrayCase arr:
            {
                var elements = arr.Value;
                if (elements.Count == 2)
                {
                    var first = elements[0].Case;
                    if (first is CborCase.UnsignedCase unsignedCase)
                    {
                        var discriminator = unsignedCase.Value;
                        var data = elements[1].TryIntoByteString();
                        return discriminator switch
                        {
                            1 => FromEcdsa(ECPublicKey.FromData(data)),
                            2 => FromEd25519(Ed25519PublicKey.FromData(data)),
                            _ => throw BCComponentsException.InvalidData("SigningPublicKey",
                                $"invalid discriminator: {discriminator}"),
                        };
                    }
                }
                throw BCComponentsException.InvalidData("SigningPublicKey", "invalid array format");
            }
            case CborCase.TaggedCase tc:
            {
                var tagValue = tc.Tag.Value;

                if (tagValue == BcTags.TagSshTextPublicKey)
                {
                    var text = tc.Item.TryIntoText();
                    return FromSsh(text);
                }

                if (tagValue == BcTags.TagMldsaPublicKey)
                {
                    var key = MLDSAPublicKey.FromTaggedCbor(cbor);
                    return FromMldsa(key);
                }

                throw BCComponentsException.InvalidData("SigningPublicKey", $"unknown tag: {tagValue}");
            }
            default:
                throw BCComponentsException.InvalidData("SigningPublicKey", "invalid CBOR format");
        }
    }

    /// <summary>
    /// Decodes a <see cref="SigningPublicKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="SigningPublicKey"/>.</returns>
    public static SigningPublicKey FromTaggedCbor(Cbor cbor)
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
            KeyVariant.Schnorr => _schnorrKey!.ToString(),
            KeyVariant.Ecdsa => _ecdsaKey!.ToString(),
            KeyVariant.Ed25519 => _ed25519Key!.ToString(),
            KeyVariant.Ssh => "SSH",
            KeyVariant.Mldsa => $"MLDSA({_mldsaKey!.Level})",
            _ => "Unknown",
        };
        return $"SigningPublicKey({((IReferenceProvider)this).RefHexShort()}, {inner})";
    }
}
