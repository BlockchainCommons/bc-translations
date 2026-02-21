namespace BlockchainCommons.DCbor;

/// <summary>
/// Defines CBOR tagged encoding for a type.
/// Requires <see cref="ICborTagged"/> for tag association.
/// </summary>
public interface ICborTaggedEncodable : ICborEncodable
{
    /// <summary>Returns the CBOR representation without the tag wrapper.</summary>
    Cbor UntaggedCbor();

    /// <summary>Returns the CBOR representation wrapped with the preferred tag.</summary>
    Cbor TaggedCbor();

    /// <summary>Returns the encoded CBOR bytes of the tagged representation.</summary>
    byte[] TaggedCborData() => TaggedCbor().ToCborData();
}
