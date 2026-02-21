namespace BlockchainCommons.DCbor;

/// <summary>
/// Defines the ability to decode a value from CBOR.
/// Implementors provide a static factory method <c>FromCbor(Cbor)</c>.
/// Since C# interfaces cannot require static methods in older language versions,
/// this is a marker interface. Implementors should provide:
/// <code>public static T FromCbor(Cbor cbor)</code>
/// </summary>
public interface ICborDecodable
{
}
