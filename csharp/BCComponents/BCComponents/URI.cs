using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A validated Uniform Resource Identifier (URI).
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="URI"/> is a string of characters that unambiguously identifies a
/// particular resource. This implementation validates URIs using <see cref="System.Uri"/>
/// to ensure conformance to the URI specification.
/// </para>
/// <para>
/// Note: This is the bc-components URI type, not <see cref="System.Uri"/>.
/// </para>
/// </remarks>
public sealed class URI : IEquatable<URI>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>Gets the URI string.</summary>
    public string Value { get; }

    private URI(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="URI"/> from a string, validating it.
    /// </summary>
    /// <param name="uri">The URI string to validate and wrap.</param>
    /// <returns>A new <see cref="URI"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the string is not a valid absolute URI.
    /// </exception>
    public static URI FromString(string uri)
    {
        if (!System.Uri.TryCreate(uri, UriKind.Absolute, out _))
        {
            throw BCComponentsException.InvalidData("URI", $"invalid URI format: {uri}");
        }
        return new URI(uri);
    }

    // --- IEquatable<URI> ---

    /// <inheritdoc/>
    public bool Equals(URI? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is URI u && Equals(u);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>Tests equality of two URI instances.</summary>
    public static bool operator ==(URI? left, URI? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two URI instances.</summary>
    public static bool operator !=(URI? left, URI? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (32).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagUri);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a text string).</summary>
    public Cbor UntaggedCbor() => Cbor.FromString(Value);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes a <see cref="URI"/> from untagged CBOR (a text string).</summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="URI"/>.</returns>
    public static URI FromUntaggedCbor(Cbor cbor)
    {
        var text = cbor.TryIntoText();
        return FromString(text);
    }

    /// <summary>Decodes a <see cref="URI"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="URI"/>.</returns>
    public static URI FromTaggedCbor(Cbor cbor)
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
    public override string ToString() => Value;
}
