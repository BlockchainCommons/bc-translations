using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A ciphertext produced by a key encapsulation mechanism (KEM).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EncapsulationCiphertext"/> represents the output of a key
/// encapsulation operation where a shared secret has been encapsulated for
/// secure transmission. The ciphertext can only be used to recover the shared
/// secret by the holder of the corresponding private key.
/// </para>
/// <list type="bullet">
/// <item>For X25519: This is the ephemeral public key generated during encapsulation.</item>
/// <item>For ML-KEM: This is the ML-KEM ciphertext.</item>
/// </list>
/// </remarks>
public sealed class EncapsulationCiphertext : IEquatable<EncapsulationCiphertext>
{
    private readonly X25519PublicKey? _x25519;
    private readonly MLKEMCiphertext? _mlkem;

    private EncapsulationCiphertext(X25519PublicKey x25519)
    {
        _x25519 = x25519;
        _mlkem = null;
    }

    private EncapsulationCiphertext(MLKEMCiphertext mlkem)
    {
        _x25519 = null;
        _mlkem = mlkem;
    }

    /// <summary>
    /// Creates an <see cref="EncapsulationCiphertext"/> wrapping an X25519 ephemeral public key.
    /// </summary>
    /// <param name="publicKey">The X25519 ephemeral public key.</param>
    /// <returns>A new <see cref="EncapsulationCiphertext"/>.</returns>
    public static EncapsulationCiphertext FromX25519(X25519PublicKey publicKey) => new(publicKey);

    /// <summary>
    /// Creates an <see cref="EncapsulationCiphertext"/> wrapping an ML-KEM ciphertext.
    /// </summary>
    /// <param name="ciphertext">The ML-KEM ciphertext.</param>
    /// <returns>A new <see cref="EncapsulationCiphertext"/>.</returns>
    public static EncapsulationCiphertext FromMLKEM(MLKEMCiphertext ciphertext) => new(ciphertext);

    /// <summary>Gets a value indicating whether this is an X25519 ciphertext.</summary>
    public bool IsX25519 => _x25519 is not null;

    /// <summary>Gets a value indicating whether this is an ML-KEM ciphertext.</summary>
    public bool IsMLKEM => _mlkem is not null;

    /// <summary>
    /// Gets the X25519 public key if this is an X25519 ciphertext.
    /// </summary>
    /// <exception cref="BCComponentsException">
    /// Thrown if this is not an X25519 ciphertext.
    /// </exception>
    public X25519PublicKey X25519PublicKey =>
        _x25519 ?? throw BCComponentsException.Crypto("Invalid key encapsulation type");

    /// <summary>
    /// Gets the ML-KEM ciphertext if this is an ML-KEM ciphertext.
    /// </summary>
    /// <exception cref="BCComponentsException">
    /// Thrown if this is not an ML-KEM ciphertext.
    /// </exception>
    public MLKEMCiphertext MLKEMCiphertextValue =>
        _mlkem ?? throw BCComponentsException.Crypto("Invalid key encapsulation type");

    /// <summary>Gets the encapsulation scheme associated with this ciphertext.</summary>
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

    // --- CBOR Serialization ---

    /// <summary>
    /// Returns the CBOR representation. The inner value's tagged CBOR is used
    /// directly (no extra wrapper tag).
    /// </summary>
    public Cbor ToCbor()
    {
        if (_x25519 is not null) return _x25519.TaggedCbor();
        return _mlkem!.TaggedCbor();
    }

    /// <summary>
    /// Decodes an <see cref="EncapsulationCiphertext"/> from a tagged CBOR value.
    /// Dispatches on the CBOR tag to determine the inner type.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="EncapsulationCiphertext"/>.</returns>
    /// <exception cref="CborWrongTypeException">
    /// Thrown if the CBOR tag does not match a known encapsulation ciphertext type.
    /// </exception>
    public static EncapsulationCiphertext FromCbor(Cbor cbor)
    {
        var taggedValue = cbor.AsTaggedValue();
        if (taggedValue is null)
            throw new CborWrongTypeException();

        var (tag, _) = taggedValue.Value;

        if (tag.Value == BcTags.TagX25519PublicKey)
            return FromX25519(X25519PublicKey.FromTaggedCbor(cbor));

        if (tag.Value == BcTags.TagMlkemCiphertext)
            return FromMLKEM(MLKEMCiphertext.FromTaggedCbor(cbor));

        throw new CborWrongTypeException();
    }

    // --- IEquatable<EncapsulationCiphertext> ---

    /// <inheritdoc/>
    public bool Equals(EncapsulationCiphertext? other)
    {
        if (other is null) return false;
        if (_x25519 is not null && other._x25519 is not null)
            return _x25519.Equals(other._x25519);
        if (_mlkem is not null && other._mlkem is not null)
            return _mlkem.Equals(other._mlkem);
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EncapsulationCiphertext ct && Equals(ct);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_x25519 is not null) return _x25519.GetHashCode();
        return _mlkem!.GetHashCode();
    }

    /// <summary>Tests equality of two EncapsulationCiphertext instances.</summary>
    public static bool operator ==(EncapsulationCiphertext? left, EncapsulationCiphertext? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two EncapsulationCiphertext instances.</summary>
    public static bool operator !=(EncapsulationCiphertext? left, EncapsulationCiphertext? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_x25519 is not null)
            return $"EncapsulationCiphertext(X25519, {_x25519})";
        return $"EncapsulationCiphertext({_mlkem!.Level}, {_mlkem})";
    }
}
