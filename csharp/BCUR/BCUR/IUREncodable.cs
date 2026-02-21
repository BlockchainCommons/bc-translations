using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR;

/// <summary>
/// A type that can be encoded to a UR. Requires the type to be CBOR-tagged encodable.
/// </summary>
public interface IUREncodable : ICborTaggedEncodable
{
}

/// <summary>
/// Extension methods for IUREncodable types.
/// </summary>
public static class UREncodableExtensions
{
    /// <summary>
    /// Returns the UR representation of this object.
    /// The type must implement ICborTagged and have at least one CBOR tag with a name.
    /// </summary>
    public static UR ToUR<T>(this T self) where T : IUREncodable, ICborTagged
    {
        var tag = T.CborTags[0];
        var name = tag.Name ?? throw new InvalidOperationException(
            $"CBOR tag {tag.Value} must have a name. Did you call RegisterTags()?");
        return UR.Create(name, self.UntaggedCbor());
    }

    /// <summary>
    /// Returns the UR string representation of this object.
    /// </summary>
    public static string ToURString<T>(this T self) where T : IUREncodable, ICborTagged
    {
        return self.ToUR().ToUrString();
    }
}
