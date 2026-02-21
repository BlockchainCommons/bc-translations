namespace BlockchainCommons.DCbor;

/// <summary>
/// Marker interface for types that implement both <see cref="ICborTaggedEncodable"/>
/// and <see cref="ICborTaggedDecodable"/>.
/// </summary>
public interface ICborTaggedCodable : ICborTaggedEncodable, ICborTaggedDecodable
{
}
