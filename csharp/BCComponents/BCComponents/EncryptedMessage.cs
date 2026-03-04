using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A secure encrypted message using IETF ChaCha20-Poly1305 authenticated
/// encryption.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EncryptedMessage"/> represents data that has been encrypted
/// using a symmetric key with the ChaCha20-Poly1305 AEAD (Authenticated
/// Encryption with Associated Data) construction as specified in
/// <see href="https://datatracker.ietf.org/doc/html/rfc8439">RFC 8439</see>.
/// </para>
/// <para>
/// An <see cref="EncryptedMessage"/> contains:
/// <list type="bullet">
/// <item><c>ciphertext</c>: The encrypted data (same length as the original plaintext)</item>
/// <item><c>aad</c>: Additional Authenticated Data that is not encrypted but is authenticated (optional)</item>
/// <item><c>nonce</c>: A 12-byte number used once for this specific encryption operation</item>
/// <item><c>auth</c>: A 16-byte authentication tag that verifies the integrity of the message</item>
/// </list>
/// </para>
/// <para>
/// The <c>aad</c> field is often used to include the <see cref="Digest"/> of the
/// plaintext, which allows verification of the plaintext after decryption and
/// preserves the unique identity of the data when used with structures like
/// Gordian Envelope.
/// </para>
/// <para>
/// CDDL:
/// <code>
/// EncryptedMessage =
///     #6.40002([ ciphertext: bstr, nonce: bstr, auth: bstr, ? aad: bstr ])
/// </code>
/// </para>
/// </remarks>
public sealed class EncryptedMessage : IEquatable<EncryptedMessage>, ICborTaggedEncodable, ICborTaggedDecodable, IDigestProvider
{
    private readonly byte[] _ciphertext;
    private readonly byte[] _aad;

    /// <summary>
    /// Creates a new <see cref="EncryptedMessage"/> from its component parts.
    /// </summary>
    /// <param name="ciphertext">The encrypted data.</param>
    /// <param name="aad">The additional authenticated data (may be empty).</param>
    /// <param name="nonce">The nonce used for encryption.</param>
    /// <param name="auth">The authentication tag.</param>
    public EncryptedMessage(byte[] ciphertext, byte[] aad, Nonce nonce, AuthenticationTag auth)
    {
        _ciphertext = (byte[])ciphertext.Clone();
        _aad = (byte[])aad.Clone();
        Nonce = nonce;
        AuthenticationTag = auth;
    }

    /// <summary>Gets a copy of the ciphertext data.</summary>
    public byte[] Ciphertext
    {
        get
        {
            var copy = new byte[_ciphertext.Length];
            Array.Copy(_ciphertext, copy, _ciphertext.Length);
            return copy;
        }
    }

    /// <summary>Gets a copy of the additional authenticated data (AAD).</summary>
    public byte[] Aad
    {
        get
        {
            var copy = new byte[_aad.Length];
            Array.Copy(_aad, copy, _aad.Length);
            return copy;
        }
    }

    /// <summary>Gets the nonce used for encryption.</summary>
    public Nonce Nonce { get; }

    /// <summary>Gets the authentication tag.</summary>
    public AuthenticationTag AuthenticationTag { get; }

    /// <summary>
    /// Returns a CBOR representation parsed from the AAD field, if it exists
    /// and is valid CBOR.
    /// </summary>
    /// <returns>
    /// A <see cref="Cbor"/> value if the AAD is non-empty and valid CBOR;
    /// otherwise <c>null</c>.
    /// </returns>
    public Cbor? AadCbor()
    {
        if (_aad.Length == 0) return null;
        try
        {
            return Cbor.TryFromData(_aad);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a <see cref="Digest"/> if the AAD data can be parsed as tagged
    /// CBOR containing a digest.
    /// </summary>
    /// <returns>
    /// A <see cref="Digest"/> if the AAD contains a valid tagged digest;
    /// otherwise <c>null</c>.
    /// </returns>
    public Digest? AadDigest()
    {
        var cbor = AadCbor();
        if (cbor is null) return null;
        try
        {
            return Digest.FromTaggedCbor(cbor);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Gets a value indicating whether the AAD data contains a valid digest.</summary>
    public bool HasDigest => AadDigest() is not null;

    // --- IDigestProvider ---

    /// <summary>
    /// Returns the digest from the AAD field.
    /// </summary>
    /// <returns>The <see cref="Digest"/> from the AAD.</returns>
    /// <exception cref="BCComponentsException">Thrown if the AAD does not contain a valid digest.</exception>
    Digest IDigestProvider.GetDigest() =>
        AadDigest() ?? throw BCComponentsException.InvalidData("EncryptedMessage", "no digest in AAD");

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the EncryptedMessage type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagEncrypted);

    // --- ICborTaggedEncodable ---

    /// <summary>
    /// Returns the untagged CBOR representation (an array).
    /// </summary>
    /// <remarks>
    /// The array contains [ciphertext, nonce, auth, ?aad]. The AAD element
    /// is only included if it is non-empty.
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Cbor.ToByteString(_ciphertext),
            Cbor.ToByteString(Nonce.Data),
            Cbor.ToByteString(AuthenticationTag.Data),
        };
        if (_aad.Length > 0)
        {
            elements.Add(Cbor.ToByteString(_aad));
        }
        return Cbor.FromList(elements);
    }

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes an <see cref="EncryptedMessage"/> from an untagged CBOR array.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value (must be an array of 3 or 4 elements).</param>
    /// <returns>A new <see cref="EncryptedMessage"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if the array does not have 3 or 4 elements.</exception>
    public static EncryptedMessage FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count < 3)
            throw BCComponentsException.InvalidData("EncryptedMessage", $"must have at least 3 elements, got {elements.Count}");

        var ciphertext = elements[0].TryIntoByteString();
        var nonceData = elements[1].TryIntoByteString();
        var nonce = Nonce.FromData(nonceData);
        var authData = elements[2].TryIntoByteString();
        var auth = AuthenticationTag.FromData(authData);
        var aad = elements.Count > 3 ? elements[3].TryIntoByteString() : Array.Empty<byte>();

        return new EncryptedMessage(ciphertext, aad, nonce, auth);
    }

    /// <summary>
    /// Decodes an <see cref="EncryptedMessage"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="EncryptedMessage"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static EncryptedMessage FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<EncryptedMessage> ---

    /// <inheritdoc/>
    public bool Equals(EncryptedMessage? other)
    {
        if (other is null) return false;
        return _ciphertext.AsSpan().SequenceEqual(other._ciphertext)
            && _aad.AsSpan().SequenceEqual(other._aad)
            && Nonce == other.Nonce
            && AuthenticationTag == other.AuthenticationTag;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EncryptedMessage m && Equals(m);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _ciphertext)
            hash.Add(b);
        foreach (var b in _aad)
            hash.Add(b);
        hash.Add(Nonce);
        hash.Add(AuthenticationTag);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two EncryptedMessage instances.</summary>
    public static bool operator ==(EncryptedMessage? left, EncryptedMessage? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two EncryptedMessage instances.</summary>
    public static bool operator !=(EncryptedMessage? left, EncryptedMessage? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString()
    {
        var ct = Convert.ToHexString(_ciphertext).ToLowerInvariant();
        var aad = Convert.ToHexString(_aad).ToLowerInvariant();
        return $"EncryptedMessage(ciphertext={ct}, aad={aad}, nonce={Nonce}, auth={AuthenticationTag})";
    }
}
