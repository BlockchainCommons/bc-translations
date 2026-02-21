namespace BlockchainCommons.DCbor;

/// <summary>
/// Marker interface for types that implement both <see cref="ICborEncodable"/>
/// and <see cref="ICborDecodable"/>.
/// </summary>
public interface ICborCodable : ICborEncodable, ICborDecodable
{
}
