using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A cryptographic seed for deterministic key generation.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="Seed"/> is a source of entropy used to generate cryptographic
/// keys in a deterministic manner. Unlike randomly generated keys, seed-derived
/// keys can be recreated if you have the original seed, making them useful for
/// backup and recovery scenarios.
/// </para>
/// <para>
/// This implementation includes the random seed data as well as optional
/// metadata: a name, a note, and a creation date. The minimum seed length is
/// 16 bytes to ensure sufficient security and entropy.
/// </para>
/// </remarks>
public sealed class Seed : IEquatable<Seed>, ICborTaggedEncodable, IPrivateKeyDataProvider
{
    /// <summary>The minimum seed length in bytes.</summary>
    public const int MinSeedLength = 16;

    /// <summary>Legacy CBOR tag value (300) for backward compatibility.</summary>
    private const ulong TagSeedV1 = 300;

    private readonly byte[] _data;

    private Seed(byte[] data, string name, string note, CborDate? creationDate)
    {
        _data = data;
        Name = name;
        Note = note;
        CreationDate = creationDate;
    }

    /// <summary>Creates a new random seed with the default length (16 bytes).</summary>
    /// <returns>A new <see cref="Seed"/> instance.</returns>
    public static Seed Create() => CreateWithLength(MinSeedLength);

    /// <summary>Creates a new random seed with the specified length.</summary>
    /// <param name="count">The number of random bytes to generate.</param>
    /// <returns>A new <see cref="Seed"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="count"/> is less than <see cref="MinSeedLength"/>.
    /// </exception>
    public static Seed CreateWithLength(int count)
    {
        return CreateWithLengthUsing(count, SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Creates a new random seed with the specified length using the given RNG.
    /// </summary>
    /// <param name="count">The number of random bytes to generate.</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new <see cref="Seed"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="count"/> is less than <see cref="MinSeedLength"/>.
    /// </exception>
    public static Seed CreateWithLengthUsing(int count, IRandomNumberGenerator rng)
    {
        var data = rng.RandomData(count);
        return Create(data);
    }

    /// <summary>Creates a new seed from the given data and optional metadata.</summary>
    /// <param name="data">The seed data (must be at least <see cref="MinSeedLength"/> bytes).</param>
    /// <param name="name">Optional name for the seed.</param>
    /// <param name="note">Optional note for the seed.</param>
    /// <param name="creationDate">Optional creation date.</param>
    /// <returns>A new <see cref="Seed"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="data"/> is shorter than <see cref="MinSeedLength"/> bytes.
    /// </exception>
    public static Seed Create(
        byte[] data,
        string? name = null,
        string? note = null,
        CborDate? creationDate = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length < MinSeedLength)
        {
            throw BCComponentsException.DataTooShort("seed", MinSeedLength, data.Length);
        }
        return new Seed(
            (byte[])data.Clone(),
            name ?? string.Empty,
            note ?? string.Empty,
            creationDate);
    }

    /// <summary>Gets a copy of the seed data as a byte array.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Gets the name of the seed.</summary>
    public string Name { get; init; }

    /// <summary>Gets the note for the seed.</summary>
    public string Note { get; init; }

    /// <summary>Gets the optional creation date of the seed.</summary>
    public CborDate? CreationDate { get; init; }

    // --- IPrivateKeyDataProvider ---

    /// <inheritdoc/>
    public byte[] PrivateKeyData() => AsBytes();

    // --- CBOR ---

    /// <summary>Gets the CBOR tags for this type (current and legacy).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagSeed, TagSeedV1);

    /// <inheritdoc/>
    public Cbor UntaggedCbor()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromInt(1), Cbor.ToByteString(_data));
        if (CreationDate is { } date)
        {
            map.Insert(Cbor.FromInt(2), date.TaggedCbor());
        }
        if (!string.IsNullOrEmpty(Name))
        {
            map.Insert(Cbor.FromInt(3), Cbor.FromString(Name));
        }
        if (!string.IsNullOrEmpty(Note))
        {
            map.Insert(Cbor.FromInt(4), Cbor.FromString(Note));
        }
        return new Cbor(CborCase.Map(map));
    }

    /// <inheritdoc/>
    public Cbor ToCbor() => TaggedCbor();

    /// <inheritdoc/>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <inheritdoc/>
    public byte[] TaggedCborData() => TaggedCbor().ToCborData();

    /// <summary>Decodes a <see cref="Seed"/> from untagged CBOR.</summary>
    /// <param name="cbor">The CBOR value to decode.</param>
    /// <returns>The decoded <see cref="Seed"/> instance.</returns>
    public static Seed FromUntaggedCbor(Cbor cbor)
    {
        var map = cbor.TryIntoMap();

        var dataCbor = map.Extract(Cbor.FromInt(1));
        var data = dataCbor.TryIntoByteString();
        if (data.Length == 0)
        {
            throw BCComponentsException.InvalidData("Seed", "data is empty");
        }

        CborDate? creationDate = null;
        var dateCbor = map.GetValue(Cbor.FromInt(2));
        if (dateCbor is not null)
        {
            creationDate = CborDate.FromTaggedCbor(dateCbor);
        }

        string? name = null;
        var nameCbor = map.GetValue(Cbor.FromInt(3));
        if (nameCbor is not null)
        {
            name = nameCbor.TryIntoText();
        }

        string? note = null;
        var noteCbor = map.GetValue(Cbor.FromInt(4));
        if (noteCbor is not null)
        {
            note = noteCbor.TryIntoText();
        }

        return Create(data, name, note, creationDate);
    }

    /// <summary>Decodes a <see cref="Seed"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value to decode.</param>
    /// <returns>The decoded <see cref="Seed"/> instance.</returns>
    public static Seed FromTaggedCbor(Cbor cbor)
    {
        var tags = CborTags;
        foreach (var tag in tags)
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

    // --- Equality ---

    /// <inheritdoc/>
    public bool Equals(Seed? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _data.AsSpan().SequenceEqual(other._data)
            && Name == other.Name
            && Note == other.Note
            && Equals(CreationDate, other.CreationDate);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Seed);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data) hash.Add(b);
        hash.Add(Name);
        hash.Add(Note);
        hash.Add(CreationDate);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() => $"Seed({_data.Length} bytes)";
}
