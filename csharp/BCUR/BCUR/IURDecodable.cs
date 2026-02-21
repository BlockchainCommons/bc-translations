using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR;

/// <summary>
/// A type that can be decoded from a UR. Requires the type to be CBOR-tagged decodable.
/// </summary>
public interface IURDecodable : ICborTaggedDecodable
{
}

/// <summary>
/// Extension methods for IURDecodable types.
/// </summary>
public static class URDecodableExtensions
{
    /// <summary>
    /// Decodes a UR into an object of this type, verifying the UR type matches.
    /// </summary>
    public static T FromUR<T>(UR ur, Func<Cbor, T> fromUntaggedCbor) where T : ICborTagged
    {
        var tag = T.CborTags[0];
        var name = tag.Name ?? throw new InvalidOperationException(
            $"CBOR tag {tag.Value} must have a name. Did you call RegisterTags()?");
        ur.CheckType(name);
        return fromUntaggedCbor(ur.Cbor);
    }

    /// <summary>
    /// Decodes a UR string into an object of this type.
    /// </summary>
    public static T FromURString<T>(string urString, Func<Cbor, T> fromUntaggedCbor) where T : ICborTagged
    {
        var ur = UR.FromUrString(urString);
        return FromUR(ur, fromUntaggedCbor);
    }
}
