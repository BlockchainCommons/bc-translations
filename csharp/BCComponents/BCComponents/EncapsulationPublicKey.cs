using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A public key used for key encapsulation mechanisms (KEM).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EncapsulationPublicKey"/> is a wrapper representing different types
/// of public keys that can be used for key encapsulation, including:
/// </para>
/// <list type="bullet">
/// <item>X25519: Curve25519-based key exchange</item>
/// <item>ML-KEM: Module Lattice-based Key Encapsulation Mechanism at various
/// security levels</item>
/// </list>
/// <para>
/// These public keys are used to encrypt (encapsulate) shared secrets that can
/// only be decrypted (decapsulated) by the corresponding private key holder.
/// </para>
/// </remarks>
public sealed class EncapsulationPublicKey : IEquatable<EncapsulationPublicKey>, IEncrypter, IReferenceProvider
{
    private readonly X25519PublicKey? _x25519;
    private readonly MLKEMPublicKey? _mlkem;

    private EncapsulationPublicKey(X25519PublicKey x25519)
    {
        _x25519 = x25519;
        _mlkem = null;
    }

    private EncapsulationPublicKey(MLKEMPublicKey mlkem)
    {
        _x25519 = null;
        _mlkem = mlkem;
    }

    /// <summary>
    /// Creates an <see cref="EncapsulationPublicKey"/> wrapping an X25519 public key.
    /// </summary>
    /// <param name="key">The X25519 public key.</param>
    /// <returns>A new <see cref="EncapsulationPublicKey"/>.</returns>
    public static EncapsulationPublicKey FromX25519(X25519PublicKey key) => new(key);

    /// <summary>
    /// Creates an <see cref="EncapsulationPublicKey"/> wrapping an ML-KEM public key.
    /// </summary>
    /// <param name="key">The ML-KEM public key.</param>
    /// <returns>A new <see cref="EncapsulationPublicKey"/>.</returns>
    public static EncapsulationPublicKey FromMLKEM(MLKEMPublicKey key) => new(key);

    /// <summary>Gets a value indicating whether this is an X25519 key.</summary>
    public bool IsX25519 => _x25519 is not null;

    /// <summary>Gets a value indicating whether this is an ML-KEM key.</summary>
    public bool IsMLKEM => _mlkem is not null;

    /// <summary>Gets the encapsulation scheme associated with this public key.</summary>
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
    /// Encapsulates a new shared secret using this public key.
    /// </summary>
    /// <remarks>
    /// <para>The encapsulation process differs based on the key type:</para>
    /// <list type="bullet">
    /// <item>For X25519: Generates an ephemeral private/public key pair, derives a
    /// shared secret using Diffie-Hellman, and returns the shared secret along with
    /// the ephemeral public key as the ciphertext.</item>
    /// <item>For ML-KEM: Uses the KEM encapsulation algorithm to generate and
    /// encapsulate a random shared secret.</item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A tuple containing the shared <see cref="SymmetricKey"/> and the
    /// <see cref="EncapsulationCiphertext"/>.
    /// </returns>
    public (SymmetricKey SharedKey, EncapsulationCiphertext Ciphertext) EncapsulateNewSharedSecret()
    {
        if (_x25519 is not null)
        {
            // Generate an ephemeral X25519 keypair
            var (ephemeralPrivateKey, ephemeralPublicKey) = X25519PrivateKey.Keypair();
            var sharedKey = ephemeralPrivateKey.SharedKeyWith(_x25519);
            return (sharedKey, EncapsulationCiphertext.FromX25519(ephemeralPublicKey));
        }

        // ML-KEM encapsulation
        var (mlkemSharedKey, mlkemCiphertext) = _mlkem!.EncapsulateNewSharedSecret();
        return (mlkemSharedKey, EncapsulationCiphertext.FromMLKEM(mlkemCiphertext));
    }

    // --- IEncrypter ---

    /// <inheritdoc/>
    EncapsulationPublicKey IEncrypter.EncapsulationPublicKey() => this;

    /// <inheritdoc/>
    (SymmetricKey SharedKey, EncapsulationCiphertext Ciphertext) IEncrypter.EncapsulateNewSharedSecret() =>
        EncapsulateNewSharedSecret();

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
    /// Decodes an <see cref="EncapsulationPublicKey"/> from a tagged CBOR value.
    /// Dispatches on the CBOR tag to determine the inner key type.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="EncapsulationPublicKey"/>.</returns>
    /// <exception cref="CborWrongTypeException">
    /// Thrown if the CBOR tag does not match a known encapsulation public key type.
    /// </exception>
    public static EncapsulationPublicKey FromCbor(Cbor cbor)
    {
        var taggedValue = cbor.AsTaggedValue();
        if (taggedValue is null)
            throw new CborWrongTypeException();

        var (tag, _) = taggedValue.Value;

        if (tag.Value == BcTags.TagX25519PublicKey)
            return FromX25519(X25519PublicKey.FromTaggedCbor(cbor));

        if (tag.Value == BcTags.TagMlkemPublicKey)
            return FromMLKEM(MLKEMPublicKey.FromTaggedCbor(cbor));

        throw new CborWrongTypeException();
    }

    // --- IEquatable<EncapsulationPublicKey> ---

    /// <inheritdoc/>
    public bool Equals(EncapsulationPublicKey? other)
    {
        if (other is null) return false;
        if (_x25519 is not null && other._x25519 is not null)
            return _x25519.Equals(other._x25519);
        if (_mlkem is not null && other._mlkem is not null)
            return _mlkem.Equals(other._mlkem);
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EncapsulationPublicKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_x25519 is not null) return _x25519.GetHashCode();
        return _mlkem!.GetHashCode();
    }

    /// <summary>Tests equality of two EncapsulationPublicKey instances.</summary>
    public static bool operator ==(EncapsulationPublicKey? left, EncapsulationPublicKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two EncapsulationPublicKey instances.</summary>
    public static bool operator !=(EncapsulationPublicKey? left, EncapsulationPublicKey? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString()
    {
        var inner = _x25519 is not null ? _x25519.ToString() : _mlkem!.ToString();
        return $"EncapsulationPublicKey({((IReferenceProvider)this).RefHexShort()}, {inner})";
    }
}
