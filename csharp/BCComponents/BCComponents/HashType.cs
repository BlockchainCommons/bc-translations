using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// Supported hash types for key derivation functions.
/// </summary>
/// <remarks>
/// CDDL:
/// <code>
/// HashType = SHA256 / SHA512
/// SHA256 = 0
/// SHA512 = 1
/// </code>
/// </remarks>
public enum HashType
{
    /// <summary>SHA-256 hash algorithm.</summary>
    SHA256 = 0,

    /// <summary>SHA-512 hash algorithm.</summary>
    SHA512 = 1,
}

/// <summary>
/// Extension methods for <see cref="HashType"/>.
/// </summary>
public static class HashTypeExtensions
{
    /// <summary>Encodes this hash type as a CBOR integer.</summary>
    public static Cbor ToCbor(this HashType hashType) =>
        Cbor.FromInt((int)hashType);

    /// <summary>
    /// Decodes a <see cref="HashType"/> from a CBOR integer value.
    /// </summary>
    /// <param name="value">The integer CBOR value.</param>
    /// <returns>The corresponding <see cref="HashType"/>.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if the value does not correspond to a known hash type.
    /// </exception>
    public static HashType FromCborValue(int value) => value switch
    {
        0 => HashType.SHA256,
        1 => HashType.SHA512,
        _ => throw BCComponentsException.General($"Invalid HashType: {value}"),
    };

    /// <summary>Decodes a <see cref="HashType"/> from a CBOR item.</summary>
    public static HashType FromCbor(Cbor cbor) =>
        FromCborValue((int)cbor.TryIntoUInt64());
}
