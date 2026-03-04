using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A symmetric content key encrypted using secret-based key derivation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EncryptedKey"/> wraps an <see cref="EncryptedMessage"/> whose
/// ciphertext is the encrypted content key and whose AAD contains the
/// CBOR-encoded key derivation parameters. Multiple derivation methods are
/// supported (HKDF, PBKDF2, Scrypt, Argon2id).
/// </para>
/// <para>
/// CDDL:
/// <code>
/// EncryptedKey = #6.40027(EncryptedMessage)
/// </code>
/// </para>
/// </remarks>
public sealed class EncryptedKey : IEquatable<EncryptedKey>, ICborTaggedEncodable, ICborTaggedDecodable
{
    private readonly KeyDerivationParams _params;
    private readonly EncryptedMessage _encryptedMessage;

    private EncryptedKey(KeyDerivationParams @params, EncryptedMessage encryptedMessage)
    {
        _params = @params;
        _encryptedMessage = encryptedMessage;
    }

    /// <summary>Returns the underlying encrypted message.</summary>
    public EncryptedMessage EncryptedMessage => _encryptedMessage;

    /// <summary>Returns <c>true</c> if the derivation method is password-based.</summary>
    public bool IsPasswordBased => _params.IsPasswordBased;

    /// <summary>Returns <c>true</c> if the derivation method is SSH agent-based.</summary>
    public bool IsSshAgent => _params.IsSshAgent;

    /// <summary>
    /// Encrypts a content key using the given key derivation method and secret
    /// (default parameters).
    /// </summary>
    /// <param name="method">The key derivation method to use.</param>
    /// <param name="secret">The secret to derive the encryption key from.</param>
    /// <param name="contentKey">The content key to encrypt.</param>
    /// <returns>A new <see cref="EncryptedKey"/>.</returns>
    public static EncryptedKey Lock(
        KeyDerivationMethod method,
        byte[] secret,
        SymmetricKey contentKey)
    {
        var @params = method switch
        {
            KeyDerivationMethod.HKDF => KeyDerivationParams.FromHKDF(new HKDFParams()),
            KeyDerivationMethod.PBKDF2 => KeyDerivationParams.FromPBKDF2(new PBKDF2Params()),
            KeyDerivationMethod.Scrypt => KeyDerivationParams.FromScrypt(new ScryptParams()),
            KeyDerivationMethod.Argon2id => KeyDerivationParams.FromArgon2id(new Argon2idParams()),
            _ => throw BCComponentsException.General($"Unknown key derivation method: {method}"),
        };
        return Lock(@params, secret, contentKey);
    }

    /// <summary>
    /// Encrypts a content key using the given key derivation parameters and secret.
    /// </summary>
    /// <param name="params">The key derivation parameters.</param>
    /// <param name="secret">The secret to derive the encryption key from.</param>
    /// <param name="contentKey">The content key to encrypt.</param>
    /// <returns>A new <see cref="EncryptedKey"/>.</returns>
    public static EncryptedKey Lock(
        KeyDerivationParams @params,
        byte[] secret,
        SymmetricKey contentKey)
    {
        var encryptedMessage = @params.Lock(contentKey, secret);
        return new EncryptedKey(@params, encryptedMessage);
    }

    /// <summary>
    /// Decrypts this encrypted key using the given secret to recover the
    /// original content key.
    /// </summary>
    /// <param name="secret">The secret to derive the decryption key from.</param>
    /// <returns>The recovered <see cref="SymmetricKey"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if decryption fails (e.g. wrong secret).
    /// </exception>
    public SymmetricKey Unlock(byte[] secret)
    {
        var aad = _encryptedMessage.Aad;
        if (aad.Length == 0)
            throw BCComponentsException.General("Missing AAD CBOR in EncryptedMessage");

        var paramsCbor = Cbor.TryFromData(aad);
        var a = paramsCbor.TryIntoArray();
        if (a.Count == 0)
            throw BCComponentsException.General("Empty KeyDerivation array in AAD");

        var index = (int)a[0].TryIntoUInt64();
        var method = KeyDerivationMethodExtensions.FromIndex(index)
            ?? throw BCComponentsException.General($"Invalid KeyDerivationMethod index: {index}");

        return method switch
        {
            KeyDerivationMethod.HKDF => HKDFParams.FromCbor(paramsCbor).Unlock(_encryptedMessage, secret),
            KeyDerivationMethod.PBKDF2 => PBKDF2Params.FromCbor(paramsCbor).Unlock(_encryptedMessage, secret),
            KeyDerivationMethod.Scrypt => ScryptParams.FromCbor(paramsCbor).Unlock(_encryptedMessage, secret),
            KeyDerivationMethod.Argon2id => Argon2idParams.FromCbor(paramsCbor).Unlock(_encryptedMessage, secret),
            _ => throw new InvalidOperationException(),
        };
    }

    // --- IEquatable<EncryptedKey> ---

    /// <inheritdoc/>
    public bool Equals(EncryptedKey? other)
    {
        if (other is null) return false;
        return _params.Equals(other._params)
            && _encryptedMessage == other._encryptedMessage;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EncryptedKey k && Equals(k);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_params, _encryptedMessage);

    /// <summary>Tests equality of two EncryptedKey instances.</summary>
    public static bool operator ==(EncryptedKey? left, EncryptedKey? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two EncryptedKey instances.</summary>
    public static bool operator !=(EncryptedKey? left, EncryptedKey? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40027).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagEncryptedKey);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation.
    /// </summary>
    /// <remarks>
    /// The untagged CBOR is the tagged CBOR of the inner <see cref="EncryptedMessage"/>.
    /// This means the wire format is:
    /// <c>#6.40027(#6.40002([ciphertext, nonce, auth, aad]))</c>.
    /// </remarks>
    public Cbor UntaggedCbor() => _encryptedMessage.TaggedCbor();

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="EncryptedKey"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (a tagged EncryptedMessage).</param>
    /// <returns>A new <see cref="EncryptedKey"/>.</returns>
    public static EncryptedKey FromUntaggedCbor(Cbor cbor)
    {
        var encryptedMessage = EncryptedMessage.FromTaggedCbor(cbor);
        var aad = encryptedMessage.Aad;
        if (aad.Length == 0)
            throw BCComponentsException.General("Missing AAD in EncryptedKey");

        var paramsCbor = Cbor.TryFromData(aad);
        var @params = KeyDerivationParams.FromCbor(paramsCbor);
        return new EncryptedKey(@params, encryptedMessage);
    }

    /// <summary>
    /// Decodes an <see cref="EncryptedKey"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="EncryptedKey"/>.</returns>
    public static EncryptedKey FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => $"EncryptedKey({_params})";
}
