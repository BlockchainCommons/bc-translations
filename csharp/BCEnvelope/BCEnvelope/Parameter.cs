using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A parameter identifier used in Gordian Envelope expressions.
/// </summary>
/// <remarks>
/// <para>
/// In Gordian Envelope, a parameter appears as a predicate in an assertion on
/// an expression envelope. The parameter identifies the name of the argument,
/// and the object of the assertion is the argument value.
/// </para>
/// <para>
/// Parameters can be identified in two ways:
/// </para>
/// <list type="number">
/// <item>By a numeric ID (for well-known parameters)</item>
/// <item>By a string name (for application-specific or less common parameters)</item>
/// </list>
/// <para>
/// When encoded in CBOR, parameters are tagged with #6.40007.
/// </para>
/// </remarks>
public sealed class Parameter
    : IEquatable<Parameter>,
      ICborTagged,
      ICborTaggedEncodable,
      ICborTaggedDecodable
{
    private readonly bool _isKnown;
    private readonly ulong _value;
    private readonly string? _name;

    private Parameter(bool isKnown, ulong value, string? name)
    {
        _isKnown = isKnown;
        _value = value;
        _name = name;
    }

    /// <summary>
    /// Creates a new well-known parameter with a numeric ID and an optional name.
    /// </summary>
    /// <param name="value">The numeric ID of the parameter.</param>
    /// <param name="name">An optional display name for the parameter.</param>
    /// <returns>A new <see cref="Parameter"/>.</returns>
    public static Parameter NewKnown(ulong value, string? name = null) =>
        new(true, value, name);

    /// <summary>
    /// Creates a new parameter identified by a string name.
    /// </summary>
    /// <param name="name">The string name of the parameter.</param>
    /// <returns>A new <see cref="Parameter"/>.</returns>
    public static Parameter NewNamed(string name) =>
        new(false, 0, name);

    /// <summary>
    /// Returns <c>true</c> if this is a well-known (numeric) parameter.
    /// </summary>
    public bool IsKnown => _isKnown;

    /// <summary>
    /// Returns <c>true</c> if this is a named (string) parameter.
    /// </summary>
    public bool IsNamed => !_isKnown;

    /// <summary>
    /// Returns the numeric value for a known parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this is a named parameter.</exception>
    public ulong Value => _isKnown
        ? _value
        : throw new InvalidOperationException("Named parameters do not have a numeric value.");

    /// <summary>
    /// Returns the display name of the parameter.
    /// </summary>
    /// <remarks>
    /// For known parameters with a name, returns the name.
    /// For known parameters without a name, returns the numeric ID as a string.
    /// For named parameters, returns the name enclosed in quotes.
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

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (#6.40007).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagParameter);

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

    /// <summary>Decodes a <see cref="Parameter"/> from untagged CBOR.</summary>
    public static Parameter FromUntaggedCbor(Cbor cbor)
    {
        return cbor.Case switch
        {
            CborCase.UnsignedCase u => NewKnown(u.Value),
            CborCase.TextCase t => NewNamed(t.Value),
            _ => throw new CborException("invalid parameter"),
        };
    }

    /// <summary>Decodes a <see cref="Parameter"/> from tagged CBOR.</summary>
    public static Parameter FromTaggedCbor(Cbor cbor)
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

    // --- IEquatable<Parameter> ---

    /// <summary>
    /// Tests equality. Known parameters compare by numeric ID (names ignored).
    /// Named parameters compare by name. Known and named are never equal.
    /// </summary>
    public bool Equals(Parameter? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_isKnown != other._isKnown) return false;
        return _isKnown
            ? _value == other._value
            : string.Equals(_name, other._name, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Parameter p && Equals(p);

    /// <inheritdoc/>
    public override int GetHashCode() => _isKnown
        ? _value.GetHashCode()
        : (_name ?? "").GetHashCode(StringComparison.Ordinal);

    /// <summary>Tests equality of two <see cref="Parameter"/> instances.</summary>
    public static bool operator ==(Parameter? left, Parameter? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two <see cref="Parameter"/> instances.</summary>
    public static bool operator !=(Parameter? left, Parameter? right) => !(left == right);

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => ParametersStore.NameForParameter(this, null);

    // --- Conversions ---

    /// <summary>Creates a known parameter from a numeric value.</summary>
    public static implicit operator Parameter(ulong value) => NewKnown(value);

    /// <summary>Creates a named parameter from a string.</summary>
    public static implicit operator Parameter(string name) => NewNamed(name);

    /// <summary>Converts a parameter to its tagged CBOR representation.</summary>
    public static implicit operator Cbor(Parameter p) => p.TaggedCbor();
}
