using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A secure key derivation container.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PrivateKeyBase"/> derives multiple cryptographic keys from a single
/// seed using HKDF key derivation. It provides access to signing keys (EC/Schnorr,
/// Ed25519), agreement keys (X25519), and implements the <see cref="ISigner"/>,
/// <see cref="IVerifier"/>, and <see cref="IDecrypter"/> interfaces.
/// </para>
/// <para>
/// The minimum seed length is 16 bytes to ensure sufficient entropy.
/// </para>
/// </remarks>
public sealed class PrivateKeyBase
    : IEquatable<PrivateKeyBase>,
      IPrivateKeyDataProvider,
      IPrivateKeysProvider,
      IPublicKeysProvider,
      ISigner,
      IVerifier,
      IDecrypter,
      IReferenceProvider,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    /// <summary>Minimum seed data length in bytes.</summary>
    public const int MinDataLength = 16;

    /// <summary>Default seed data length in bytes.</summary>
    public const int DefaultDataLength = 32;

    private readonly byte[] _data;

    private PrivateKeyBase(byte[] data)
    {
        _data = data;
    }

    /// <summary>Creates a new random private key base with 32 bytes of entropy.</summary>
    /// <returns>A new <see cref="PrivateKeyBase"/> instance.</returns>
    public static PrivateKeyBase New()
    {
        return NewUsing(SecureRandomNumberGenerator.Shared);
    }

    /// <summary>Creates a new random private key base using the given RNG.</summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="PrivateKeyBase"/> instance.</returns>
    public static PrivateKeyBase NewUsing(IRandomNumberGenerator rng)
    {
        var data = rng.RandomData(DefaultDataLength);
        return new PrivateKeyBase(data);
    }

    /// <summary>
    /// Restores a private key base from the given data.
    /// </summary>
    /// <param name="data">The seed data (must be at least <see cref="MinDataLength"/> bytes).</param>
    /// <returns>A new <see cref="PrivateKeyBase"/> instance.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="data"/> is shorter than <see cref="MinDataLength"/> bytes.
    /// </exception>
    public static PrivateKeyBase FromData(byte[] data)
    {
        if (data.Length < MinDataLength)
            throw BCComponentsException.DataTooShort("private key base", MinDataLength, data.Length);
        return new PrivateKeyBase((byte[])data.Clone());
    }

    /// <summary>Creates a private key base from a hexadecimal string.</summary>
    /// <param name="hex">A hexadecimal string.</param>
    /// <returns>A new <see cref="PrivateKeyBase"/> instance.</returns>
    public static PrivateKeyBase FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Gets a copy of the underlying seed data.</summary>
    public byte[] Data => (byte[])_data.Clone();

    // --- Key derivation ---

    /// <summary>Derives the secp256k1 signing private key.</summary>
    public ECPrivateKey SigningPrivateKey() =>
        ECPrivateKey.DeriveFromKeyMaterial(_data);

    /// <summary>Derives the ECDSA signing private key (alias for <see cref="SigningPrivateKey"/>).</summary>
    public ECPrivateKey EcdsaSigningPrivateKey() => SigningPrivateKey();

    /// <summary>Derives the Schnorr signing private key (alias for <see cref="SigningPrivateKey"/>).</summary>
    public ECPrivateKey SchnorrSigningPrivateKey() => SigningPrivateKey();

    /// <summary>Derives the Ed25519 signing private key.</summary>
    public Ed25519PrivateKey Ed25519SigningPrivateKey() =>
        Ed25519PrivateKey.DeriveFromKeyMaterial(_data);

    /// <summary>Derives the X25519 agreement private key.</summary>
    public X25519PrivateKey X25519AgreementPrivateKey() =>
        X25519PrivateKey.DeriveFromKeyMaterial(_data);

    /// <summary>Derives the X25519 agreement public key.</summary>
    public X25519PublicKey X25519AgreementPublicKey() =>
        X25519AgreementPrivateKey().PublicKey();

    /// <summary>Returns the default signing public key (Schnorr).</summary>
    public SigningPublicKey DefaultSigningPublicKey() =>
        SigningPublicKey.FromSchnorr(SigningPrivateKey().SchnorrPublicKey());

    /// <summary>Returns the default encapsulation public key (X25519).</summary>
    public EncapsulationPublicKey DefaultEncapsulationPublicKey() =>
        EncapsulationPublicKey.FromX25519(X25519AgreementPublicKey());

    // --- IPrivateKeyDataProvider ---

    /// <inheritdoc/>
    public byte[] PrivateKeyData() => (byte[])_data.Clone();

    // --- IPrivateKeysProvider ---

    /// <inheritdoc/>
    PrivateKeys IPrivateKeysProvider.PrivateKeys() =>
        new(
            BCComponents.SigningPrivateKey.NewSchnorr(SigningPrivateKey()),
            BCComponents.EncapsulationPrivateKey.FromX25519(X25519AgreementPrivateKey()));

    // --- IPublicKeysProvider ---

    /// <inheritdoc/>
    PublicKeys IPublicKeysProvider.PublicKeys() =>
        new(
            DefaultSigningPublicKey(),
            DefaultEncapsulationPublicKey());

    // --- ISigner ---

    /// <inheritdoc/>
    public Signature SignWithOptions(byte[] message, SigningOptions? options = null)
    {
        var signingKey = BCComponents.SigningPrivateKey.NewSchnorr(SigningPrivateKey());
        return signingKey.SignWithOptions(message, options);
    }

    // --- IVerifier ---

    /// <inheritdoc/>
    public bool Verify(Signature signature, byte[] message)
    {
        return DefaultSigningPublicKey().Verify(signature, message);
    }

    // --- IDecrypter ---

    /// <inheritdoc/>
    EncapsulationPrivateKey IDecrypter.EncapsulationPrivateKey() =>
        BCComponents.EncapsulationPrivateKey.FromX25519(X25519AgreementPrivateKey());

    /// <inheritdoc/>
    SymmetricKey IDecrypter.DecapsulateSharedSecret(EncapsulationCiphertext ciphertext) =>
        ((IDecrypter)this).EncapsulationPrivateKey().DecapsulateSharedSecret(ciphertext);

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(TaggedCbor().ToCborData()));

    // --- IEquatable<PrivateKeyBase> ---

    /// <inheritdoc/>
    public bool Equals(PrivateKeyBase? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PrivateKeyBase k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two PrivateKeyBase instances.</summary>
    public static bool operator ==(PrivateKeyBase? left, PrivateKeyBase? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two PrivateKeyBase instances.</summary>
    public static bool operator !=(PrivateKeyBase? left, PrivateKeyBase? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40016).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagPrivateKeyBase);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes a <see cref="PrivateKeyBase"/> from untagged CBOR (a byte string).</summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="PrivateKeyBase"/>.</returns>
    public static PrivateKeyBase FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>Decodes a <see cref="PrivateKeyBase"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="PrivateKeyBase"/>.</returns>
    public static PrivateKeyBase FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => $"PrivateKeyBase({((IReferenceProvider)this).RefHexShort()})";
}
