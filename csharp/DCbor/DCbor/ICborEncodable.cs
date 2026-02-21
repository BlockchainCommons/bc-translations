namespace BlockchainCommons.DCbor;

/// <summary>
/// Defines the ability to encode a value as CBOR.
/// Implement <see cref="ToCbor"/> to provide the CBOR representation.
/// </summary>
public interface ICborEncodable
{
    /// <summary>Converts this value to a CBOR representation.</summary>
    Cbor ToCbor();

    /// <summary>Converts this value to encoded CBOR bytes.</summary>
    byte[] ToCborData() => ToCbor().ToCborData();
}
