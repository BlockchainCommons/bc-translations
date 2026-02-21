namespace BlockchainCommons.DCbor;

/// <summary>
/// Marker interface for types that can be decoded from CBOR.
/// Implementors conventionally provide a static factory method:
/// <code>public static T FromCbor(Cbor cbor)</code>
/// </summary>
public interface ICborDecodable
{
}
