namespace BlockchainCommons.DCbor;

/// <summary>
/// Defines CBOR tagged decoding for a type.
/// Implementors should provide static methods:
/// <code>
/// public static T FromUntaggedCbor(Cbor cbor)
/// public static T FromTaggedCbor(Cbor cbor)
/// </code>
/// </summary>
public interface ICborTaggedDecodable : ICborDecodable
{
}
