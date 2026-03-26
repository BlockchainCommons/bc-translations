using System.Globalization;
using System.Numerics;
using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.KnownValues;

/// <summary>
/// A value in a namespace of unsigned integers that represents a stand-alone
/// ontological concept.
/// </summary>
/// <remarks>
/// Known Values provide a compact, deterministic way to represent commonly
/// used ontological concepts. Equality and hashing are based only on the raw
/// numeric value, not the assigned display name.
/// </remarks>
public sealed class KnownValue
    : IEquatable<KnownValue>,
      ICborTagged,
      ICborTaggedEncodable,
      ICborTaggedDecodable,
      IDigestProvider
{
    private readonly string? _assignedName;

    /// <summary>
    /// Creates a new <see cref="KnownValue"/> with the given numeric value and
    /// no assigned name.
    /// </summary>
    public KnownValue(ulong value)
    {
        Value = value;
        _assignedName = null;
    }

    /// <summary>
    /// Creates a new <see cref="KnownValue"/> with the given numeric value and
    /// assigned name.
    /// </summary>
    public KnownValue(ulong value, string assignedName)
    {
        ArgumentNullException.ThrowIfNull(assignedName);

        Value = value;
        _assignedName = assignedName;
    }

    /// <summary>
    /// The numeric value encoded into CBOR.
    /// </summary>
    public ulong Value { get; }

    /// <summary>
    /// The assigned name used for debugging and formatted output, if present.
    /// </summary>
    public string? AssignedName => _assignedName;

    /// <summary>
    /// Returns the assigned name, or the numeric value as a string when no
    /// assigned name exists.
    /// </summary>
    public string Name => _assignedName ?? Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Creates a new <see cref="KnownValue"/> with the given numeric value.
    /// </summary>
    public static KnownValue New(ulong value) => new(value);

    /// <summary>
    /// Creates a new <see cref="KnownValue"/> with the given numeric value and
    /// assigned name.
    /// </summary>
    public static KnownValue NewWithName<T>(T value, string assignedName)
        where T : IBinaryInteger<T>
    {
        return new KnownValue(ulong.CreateChecked(value), assignedName);
    }

    /// <summary>
    /// Creates a registry-backed <see cref="KnownValue"/> with a static name.
    /// </summary>
    public static KnownValue NewWithStaticName(ulong value, string name)
    {
        return new KnownValue(value, name);
    }

    /// <summary>
    /// Creates a copy of this <see cref="KnownValue"/>.
    /// </summary>
    public KnownValue Clone() =>
        _assignedName is null ? new KnownValue(Value) : new KnownValue(Value, _assignedName);

    /// <inheritdoc />
    public Digest GetDigest()
    {
        return Digest.FromImage(TaggedCbor().ToCborData());
    }

    /// <summary>
    /// The CBOR tags associated with <see cref="KnownValue"/>.
    /// </summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagKnownValue);

    /// <summary>
    /// Returns the untagged CBOR representation of this known value.
    /// </summary>
    public Cbor UntaggedCbor() => Cbor.FromUInt(Value);

    /// <summary>
    /// Returns the tagged CBOR representation of this known value.
    /// </summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>
    /// Returns the tagged CBOR representation.
    /// </summary>
    public Cbor ToCbor() => TaggedCbor();

    /// <summary>
    /// Decodes a <see cref="KnownValue"/> from untagged CBOR.
    /// </summary>
    public static KnownValue FromUntaggedCbor(Cbor cbor)
    {
        return new KnownValue(cbor.TryIntoUInt64());
    }

    /// <summary>
    /// Decodes a <see cref="KnownValue"/> from tagged CBOR.
    /// </summary>
    public static KnownValue FromTaggedCbor(Cbor cbor)
    {
        foreach (var tag in CborTags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException)
            {
            }
            catch (CborWrongTypeException)
            {
            }
        }

        throw new CborWrongTypeException();
    }

    /// <inheritdoc />
    public bool Equals(KnownValue? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is KnownValue other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// Converts a raw numeric value into a <see cref="KnownValue"/>.
    /// </summary>
    public static implicit operator KnownValue(ulong value) => new(value);

    /// <summary>
    /// Converts a signed integer into a <see cref="KnownValue"/>.
    /// </summary>
    public static implicit operator KnownValue(int value) =>
        new(ulong.CreateChecked(value));

    /// <summary>
    /// Converts a <see cref="KnownValue"/> into tagged CBOR.
    /// </summary>
    public static implicit operator Cbor(KnownValue value) => value.TaggedCbor();

    public static bool operator ==(KnownValue? left, KnownValue? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(KnownValue? left, KnownValue? right) =>
        !(left == right);
}
