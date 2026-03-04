using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Random salt used to decorrelate other information.
/// </summary>
/// <remarks>
/// A <see cref="Salt"/> is a cryptographic primitive consisting of random data
/// that is used to modify the output of a cryptographic function. Salts are
/// primarily used in password hashing to defend against dictionary attacks,
/// rainbow table attacks, and pre-computation attacks.
///
/// Unlike a <see cref="Nonce"/> which has a fixed size, a salt can have a
/// variable length (minimum 8 bytes). Different creation methods are provided
/// to generate salts of appropriate sizes for different use cases.
/// </remarks>
public sealed class Salt : IEquatable<Salt>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>Minimum allowed salt size in bytes.</summary>
    public const int MinSize = 8;

    private readonly byte[] _data;

    private Salt(byte[] data)
    {
        _data = data;
    }

    /// <summary>Gets the length of the salt in bytes.</summary>
    public int Length => _data.Length;

    /// <summary>Gets whether the salt is empty (not recommended).</summary>
    public bool IsEmpty => _data.Length == 0;

    /// <summary>
    /// Creates a new salt from the given byte data.
    /// </summary>
    /// <param name="data">The salt data. No minimum length check is performed.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    public static Salt FromData(byte[] data)
    {
        var copy = new byte[data.Length];
        Array.Copy(data, copy, data.Length);
        return new Salt(copy);
    }

    /// <summary>Returns the salt data as a read-only span.</summary>
    /// <returns>A read-only span over the salt bytes.</returns>
    public ReadOnlySpan<byte> AsBytes() => _data;

    /// <summary>
    /// Creates a salt of the specified length using the system's cryptographically secure RNG.
    /// </summary>
    /// <param name="count">The number of random bytes. Must be at least 8.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="count"/> is less than 8.</exception>
    public static Salt CreateWithLength(int count)
    {
        return CreateWithLengthUsing(count, SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Creates a salt of the specified length using the given RNG.
    /// </summary>
    /// <param name="count">The number of random bytes. Must be at least 8.</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="count"/> is less than 8.</exception>
    public static Salt CreateWithLengthUsing(int count, IRandomNumberGenerator rng)
    {
        if (count < MinSize)
            throw BCComponentsException.DataTooShort("salt", MinSize, count);
        return new Salt(rng.RandomData(count));
    }

    /// <summary>
    /// Creates a salt with a random length chosen from the given range, using the
    /// system's cryptographically secure RNG.
    /// </summary>
    /// <param name="minLen">The minimum salt length (inclusive). Must be at least 8.</param>
    /// <param name="maxLen">The maximum salt length (inclusive).</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="minLen"/> is less than 8.</exception>
    public static Salt CreateInRange(int minLen, int maxLen)
    {
        if (minLen < MinSize)
            throw BCComponentsException.DataTooShort("salt", MinSize, minLen);
        var rng = SecureRandomNumberGenerator.Shared;
        return CreateInRangeUsing(minLen, maxLen, rng);
    }

    /// <summary>
    /// Creates a salt with a random length chosen from the given range, using the given RNG.
    /// </summary>
    /// <param name="minLen">The minimum salt length (inclusive). Must be at least 8.</param>
    /// <param name="maxLen">The maximum salt length (inclusive).</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="BCComponentsException">Thrown if <paramref name="minLen"/> is less than 8.</exception>
    public static Salt CreateInRangeUsing(int minLen, int maxLen, IRandomNumberGenerator rng)
    {
        if (minLen < MinSize)
            throw BCComponentsException.DataTooShort("salt", MinSize, minLen);
        int count = rng.NextInClosedRange(minLen, maxLen);
        return CreateWithLengthUsing(count, rng);
    }

    /// <summary>
    /// Creates a salt generally proportionate to the size of the object being salted,
    /// using the system's cryptographically secure RNG.
    /// </summary>
    /// <param name="size">The size of the data being salted, in bytes.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    public static Salt CreateForSize(int size)
    {
        return CreateForSizeUsing(size, SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Creates a salt generally proportionate to the size of the object being salted,
    /// using the given RNG.
    /// </summary>
    /// <param name="size">The size of the data being salted, in bytes.</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    public static Salt CreateForSizeUsing(int size, IRandomNumberGenerator rng)
    {
        double count = size;
        int minSize = Math.Max(MinSize, (int)Math.Ceiling(count * 0.05));
        int maxSize = Math.Max(minSize + MinSize, (int)Math.Ceiling(count * 0.25));
        return CreateInRangeUsing(minSize, maxSize, rng);
    }

    /// <summary>
    /// Creates a new salt from a hexadecimal string.
    /// </summary>
    /// <param name="hex">A hexadecimal string.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="FormatException">Thrown if the hex string is invalid.</exception>
    public static Salt FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Gets the salt data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags associated with the Salt type.</summary>
    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagSalt);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>
    /// Decodes a <see cref="Salt"/> from an untagged CBOR byte string.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR is not a valid byte string.</exception>
    public static Salt FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return FromData(data);
    }

    /// <summary>
    /// Decodes a <see cref="Salt"/> from a tagged CBOR value.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="Salt"/>.</returns>
    /// <exception cref="CborException">Thrown if the CBOR tag does not match or the data is invalid.</exception>
    public static Salt FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<Salt> ---

    /// <inheritdoc/>
    public bool Equals(Salt? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Salt s && Equals(s);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two Salt instances.</summary>
    public static bool operator ==(Salt? left, Salt? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two Salt instances.</summary>
    public static bool operator !=(Salt? left, Salt? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"Salt({Length})";

    /// <summary>Returns a byte array copy of this salt's data.</summary>
    /// <returns>A byte array.</returns>
    public byte[] ToByteArray() => (byte[])_data.Clone();
}
