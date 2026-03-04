using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A private key used for key encapsulation mechanisms (KEM).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EncapsulationPrivateKey"/> is a wrapper representing different types
/// of private keys that can be used for key encapsulation, including:
/// </para>
/// <list type="bullet">
/// <item>X25519: Curve25519-based key exchange</item>
/// <item>ML-KEM: Module Lattice-based Key Encapsulation Mechanism at various
/// security levels</item>
/// </list>
/// <para>
/// These private keys are used to decrypt (decapsulate) shared secrets that
/// have been encapsulated with the corresponding public keys.
/// </para>
/// </remarks>
public sealed class EncapsulationPrivateKey : IEquatable<EncapsulationPrivateKey>, IDecrypter, IReferenceProvider
{
    private readonly X25519PrivateKey? _x25519;
    private readonly MLKEMPrivateKey? _mlkem;

    private EncapsulationPrivateKey(X25519PrivateKey x25519)
    {
        _x25519 = x25519;
        _mlkem = null;
    }

    private EncapsulationPrivateKey(MLKEMPrivateKey mlkem)
    {
        _x25519 = null;
        _mlkem = mlkem;
    }

    /// <summary>
    /// Creates an <see cref="EncapsulationPrivateKey"/> wrapping an X25519 private key.
    /// </summary>
    /// <param name="key">The X25519 private key.</param>
    /// <returns>A new <see cref="EncapsulationPrivateKey"/>.</returns>
    public static EncapsulationPrivateKey FromX25519(X25519PrivateKey key) => new(key);

    /// <summary>
    /// Creates an <see cref="EncapsulationPrivateKey"/> wrapping an ML-KEM private key.
    /// </summary>
    /// <param name="key">The ML-KEM private key.</param>
    /// <returns>A new <see cref="EncapsulationPrivateKey"/>.</returns>
    public static EncapsulationPrivateKey FromMLKEM(MLKEMPrivateKey key) => new(key);

    /// <summary>Gets a value indicating whether this is an X25519 key.</summary>
    public bool IsX25519 => _x25519 is not null;

    /// <summary>Gets a value indicating whether this is an ML-KEM key.</summary>
    public bool IsMLKEM => _mlkem is not null;

    /// <summary>Gets the encapsulation scheme associated with this private key.</summary>
    public EncapsulationScheme Scheme
    {
        get
        {
            if (_x25519 is not null) return EncapsulationScheme.X25519;
            return _mlkem!.Level switch
            {
                MLKEMLevel.MLKEM512 => EncapsulationScheme.MLKEM512,
                MLKEMLevel.MLKEM768 => EncapsulationScheme.MLKEM768,
                MLKEMLevel.MLKEM1024 => EncapsulationScheme.MLKEM1024,
                _ => throw new InvalidOperationException(),
            };
        }
    }

    /// <summary>
    /// Decapsulates a shared secret from a ciphertext using this private key.
    /// </summary>
    /// <param name="ciphertext">
    /// The <see cref="EncapsulationCiphertext"/> containing the encapsulated shared secret.
    /// </param>
    /// <returns>The decapsulated <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the ciphertext type does not match the private key type, or if
    /// decapsulation fails.
    /// </exception>
    public SymmetricKey DecapsulateSharedSecret(EncapsulationCiphertext ciphertext)
    {
        if (_x25519 is not null && ciphertext.IsX25519)
        {
            return _x25519.SharedKeyWith(ciphertext.X25519PublicKey);
        }

        if (_mlkem is not null && ciphertext.IsMLKEM)
        {
            return _mlkem.DecapsulateSharedSecret(ciphertext.MLKEMCiphertextValue);
        }

        throw BCComponentsException.Crypto(
            $"Mismatched key encapsulation types. private key: {Scheme}, ciphertext: {ciphertext.Scheme}");
    }

    /// <summary>
    /// Derives the corresponding <see cref="EncapsulationPublicKey"/>.
    /// Only supported for X25519 keys.
    /// </summary>
    /// <returns>The corresponding <see cref="EncapsulationPublicKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if this is an ML-KEM key (public key derivation is not supported).
    /// </exception>
    public EncapsulationPublicKey PublicKey()
    {
        if (_x25519 is not null)
            return EncapsulationPublicKey.FromX25519(_x25519.PublicKey());

        throw BCComponentsException.Crypto("Deriving ML-KEM public key not supported");
    }

    // --- IDecrypter ---

    /// <inheritdoc/>
    EncapsulationPrivateKey IDecrypter.EncapsulationPrivateKey() => this;

    /// <inheritdoc/>
    SymmetricKey IDecrypter.DecapsulateSharedSecret(EncapsulationCiphertext ciphertext) =>
        DecapsulateSharedSecret(ciphertext);

    // --- IReferenceProvider ---

    /// <inheritdoc/>
    public Reference Reference() =>
        BCComponents.Reference.FromDigest(Digest.FromImage(ToCbor().ToCborData()));

    // --- CBOR Serialization ---

    /// <summary>
    /// Returns the CBOR representation. The inner key's tagged CBOR is used
    /// directly (no extra wrapper tag).
    /// </summary>
    public Cbor ToCbor()
    {
        if (_x25519 is not null) return _x25519.TaggedCbor();
        return _mlkem!.TaggedCbor();
    }

    /// <summary>
    /// Decodes an <see cref="EncapsulationPrivateKey"/> from a tagged CBOR value.
    /// Dispatches on the CBOR tag to determine the inner key type.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="EncapsulationPrivateKey"/>.</returns>
    /// <exception cref="CborWrongTypeException">
    /// Thrown if the CBOR tag does not match a known encapsulation private key type.
    /// </exception>
    public static EncapsulationPrivateKey FromCbor(Cbor cbor)
    {
        var taggedValue = cbor.AsTaggedValue();
        if (taggedValue is null)
            throw new CborWrongTypeException();

        var (tag, _) = taggedValue.Value;

        if (tag.Value == BcTags.TagX25519PrivateKey)
            return FromX25519(X25519PrivateKey.FromTaggedCbor(cbor));

        if (tag.Value == BcTags.TagMlkemPrivateKey)
            return FromMLKEM(MLKEMPrivateKey.FromTaggedCbor(cbor));

        throw new CborWrongTypeException();
    }

    // --- IEquatable<EncapsulationPrivateKey> ---

    /// <inheritdoc/>
    public bool Equals(EncapsulationPrivateKey? other)
    {
        if (other is null) return false;
        if (_x25519 is not null && other._x25519 is not null)
            return _x25519.Equals(other._x25519);
        if (_mlkem is not null && other._mlkem is not null)
            return _mlkem.Equals(other._mlkem);
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EncapsulationPrivateKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_x25519 is not null) return _x25519.GetHashCode();
        return _mlkem!.GetHashCode();
    }

    /// <summary>Tests equality of two EncapsulationPrivateKey instances.</summary>
    public static bool operator ==(EncapsulationPrivateKey? left, EncapsulationPrivateKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two EncapsulationPrivateKey instances.</summary>
    public static bool operator !=(EncapsulationPrivateKey? left, EncapsulationPrivateKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString()
    {
        var inner = _x25519 is not null ? _x25519.ToString() : _mlkem!.ToString();
        return $"EncapsulationPrivateKey({((IReferenceProvider)this).RefHexShort()}, {inner})";
    }
}
