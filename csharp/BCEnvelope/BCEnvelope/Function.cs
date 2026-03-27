using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A function identifier used in Gordian Envelope expressions.
/// </summary>
/// <remarks>
/// <para>
/// In Gordian Envelope, a function appears as the subject of an expression
/// envelope, with its parameters as assertions on that envelope.
/// </para>
/// <para>
/// Functions can be identified in two ways:
/// </para>
/// <list type="number">
/// <item>By a numeric ID (for well-known functions)</item>
/// <item>By a string name (for application-specific or less common functions)</item>
/// </list>
/// <para>
/// When encoded in CBOR, functions are tagged with #6.40006.
/// </para>
/// </remarks>
public sealed class Function
    : IEquatable<Function>,
      ICborTagged,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    private readonly bool _isKnown;
    private readonly ulong _value;
    private readonly string? _name;

    private Function(bool isKnown, ulong value, string? name)
    {
        _isKnown = isKnown;
        _value = value;
        _name = name;
    }

    /// <summary>
    /// Creates a new well-known function with a numeric ID and an optional name.
    /// </summary>
    /// <param name="value">The numeric ID of the function.</param>
    /// <param name="name">An optional display name for the function.</param>
    /// <returns>A new <see cref="Function"/>.</returns>
    public static Function NewKnown(ulong value, string? name = null) =>
        new(true, value, name);

    /// <summary>
    /// Creates a new function identified by a string name.
    /// </summary>
    /// <param name="name">The string name of the function.</param>
    /// <returns>A new <see cref="Function"/>.</returns>
    public static Function NewNamed(string name) =>
        new(false, 0, name);

    /// <summary>
    /// Returns <c>true</c> if this is a well-known (numeric) function.
    /// </summary>
    public bool IsKnown => _isKnown;

    /// <summary>
    /// Returns <c>true</c> if this is a named (string) function.
    /// </summary>
    public bool IsNamed => !_isKnown;

    /// <summary>
    /// Returns the numeric value for a known function.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this is a named function.</exception>
    public ulong Value => _isKnown
        ? _value
        : throw new InvalidOperationException("Named functions do not have a numeric value.");

    /// <summary>
    /// Returns the display name of the function.
    /// </summary>
    /// <remarks>
    /// For known functions with a name, returns the name.
    /// For known functions without a name, returns the numeric ID as a string.
    /// For named functions, returns the name enclosed in quotes.
    /// </remarks>
    public string Name
    {
        get
        {
            if (_isKnown)
                return _name ?? _value.ToString();
            return $"\"{_name}\"";
        }
    }

    /// <summary>
    /// Returns the raw name for named functions, or <c>null</c> for known functions.
    /// </summary>
    public string? NamedName => _isKnown ? null : _name;

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (#6.40006).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagFunction);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation.</summary>
    public Cbor UntaggedCbor() => _isKnown
        ? Cbor.FromUInt(_value)
        : Cbor.FromString(_name!);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes a <see cref="Function"/> from untagged CBOR.</summary>
    public static Function FromUntaggedCbor(Cbor cbor)
    {
        return cbor.Case switch
        {
            CborCase.UnsignedCase u => NewKnown(u.Value),
            CborCase.TextCase t => NewNamed(t.Value),
            _ => throw new CborException("invalid function"),
        };
    }

    /// <summary>Decodes a <see cref="Function"/> from tagged CBOR.</summary>
    public static Function FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<Function> ---

    /// <summary>
    /// Tests equality. Known functions compare by numeric ID (names ignored).
    /// Named functions compare by name. Known and named are never equal.
    /// </summary>
    public bool Equals(Function? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_isKnown != other._isKnown) return false;
        return _isKnown
            ? _value == other._value
            : string.Equals(_name, other._name, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Function f && Equals(f);

    /// <inheritdoc/>
    public override int GetHashCode() => _isKnown
        ? _value.GetHashCode()
        : (_name ?? "").GetHashCode(StringComparison.Ordinal);

    /// <summary>Tests equality of two <see cref="Function"/> instances.</summary>
    public static bool operator ==(Function? left, Function? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two <see cref="Function"/> instances.</summary>
    public static bool operator !=(Function? left, Function? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => FunctionsStore.NameForFunction(this, null);

    // --- Conversions ---

    /// <summary>Creates a known function from a numeric value.</summary>
    public static implicit operator Function(ulong value) => NewKnown(value);

    /// <summary>Creates a named function from a string.</summary>
    public static implicit operator Function(string name) => NewNamed(name);

    /// <summary>Converts a function to its tagged CBOR representation.</summary>
    public static implicit operator Cbor(Function f) => f.TaggedCbor();
}
