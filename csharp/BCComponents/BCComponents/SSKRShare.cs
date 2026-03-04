using BlockchainCommons.BCRand;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A share of a secret split using Sharded Secret Key Reconstruction (SSKR).
/// </summary>
/// <remarks>
/// <para>
/// SSKR is a protocol for splitting a secret into multiple shares across one or
/// more groups, such that the secret can be reconstructed only when a threshold
/// number of shares from a threshold number of groups are combined.
/// </para>
/// <para>
/// Each SSKR share contains:
/// <list type="bullet">
/// <item>A unique identifier for the split</item>
/// <item>Metadata about the group structure (thresholds, counts, indices)</item>
/// <item>A portion of the secret data</item>
/// </list>
/// </para>
/// <para>
/// SSKR shares follow a specific binary format that includes a 5-byte metadata
/// header followed by the share value.
/// </para>
/// </remarks>
public sealed class SSKRShare : IEquatable<SSKRShare>, ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>Legacy CBOR tag value (309) for backward compatibility.</summary>
    private const ulong TagSskrShareV1 = 309;

    private readonly byte[] _data;

    private SSKRShare(byte[] data)
    {
        _data = data;
    }

    /// <summary>Creates a new <see cref="SSKRShare"/> from raw binary data.</summary>
    /// <param name="data">The raw binary data of the SSKR share.</param>
    /// <returns>A new <see cref="SSKRShare"/>.</returns>
    public static SSKRShare FromData(byte[] data)
    {
        return new SSKRShare((byte[])data.Clone());
    }

    /// <summary>Creates a new <see cref="SSKRShare"/> from a hexadecimal string.</summary>
    /// <param name="hex">A hexadecimal string representing the SSKR share data.</param>
    /// <returns>A new <see cref="SSKRShare"/>.</returns>
    public static SSKRShare FromHex(string hex)
    {
        return FromData(Convert.FromHexString(hex));
    }

    /// <summary>Returns a copy of the raw binary data of this share.</summary>
    public byte[] AsBytes() => (byte[])_data.Clone();

    /// <summary>Gets the data as a lowercase hexadecimal string.</summary>
    public string Hex => Convert.ToHexString(_data).ToLowerInvariant();

    /// <summary>
    /// Returns the unique identifier of the split to which this share belongs.
    /// </summary>
    /// <remarks>
    /// The identifier is a 16-bit value that is the same for all shares in a
    /// split and is used to verify that shares belong together when combining them.
    /// </remarks>
    public int Identifier() =>
        ((_data[0] & 0xFF) << 8) | (_data[1] & 0xFF);

    /// <summary>Returns the unique identifier of the split as a hexadecimal string.</summary>
    public string IdentifierHex() =>
        Convert.ToHexString(_data, 0, 2).ToLowerInvariant();

    /// <summary>
    /// Returns the minimum number of groups whose quorum must be met to
    /// reconstruct the secret.
    /// </summary>
    public int GroupThreshold() =>
        ((_data[2] & 0xFF) >> 4) + 1;

    /// <summary>Returns the total number of groups in the split.</summary>
    public int GroupCount() =>
        (_data[2] & 0x0F) + 1;

    /// <summary>Returns the zero-based index of the group to which this share belongs.</summary>
    public int GroupIndex() =>
        (_data[3] & 0xFF) >> 4;

    /// <summary>
    /// Returns the minimum number of shares within the group that must be
    /// combined to meet the group threshold.
    /// </summary>
    public int MemberThreshold() =>
        (_data[3] & 0x0F) + 1;

    /// <summary>Returns the zero-based index of this share within its group.</summary>
    public int MemberIndex() =>
        _data[4] & 0x0F;

    // --- IEquatable<SSKRShare> ---

    /// <inheritdoc/>
    public bool Equals(SSKRShare? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SSKRShare s && Equals(s);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    /// <summary>Tests equality of two SSKRShare instances.</summary>
    public static bool operator ==(SSKRShare? left, SSKRShare? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two SSKRShare instances.</summary>
    public static bool operator !=(SSKRShare? left, SSKRShare? right) => !(left == right);

    // --- ICborTagged ---

    /// <summary>Returns the CBOR tags for this type (40309 and legacy 309).</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagSskrShare, TagSskrShareV1);

    // --- ICborTaggedEncodable ---

    /// <summary>Returns the untagged CBOR representation (a byte string).</summary>
    public Cbor UntaggedCbor() => Cbor.ToByteString(_data);

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>Returns the tagged CBOR representation.</summary>
    public Cbor ToCbor() => TaggedCbor();

    // --- ICborTaggedDecodable ---

    /// <summary>Decodes an <see cref="SSKRShare"/> from untagged CBOR (a byte string).</summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A new <see cref="SSKRShare"/>.</returns>
    public static SSKRShare FromUntaggedCbor(Cbor cbor)
    {
        var data = cbor.TryIntoByteString();
        return new SSKRShare(data);
    }

    /// <summary>Decodes an <see cref="SSKRShare"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A new <see cref="SSKRShare"/>.</returns>
    public static SSKRShare FromTaggedCbor(Cbor cbor)
    {
        foreach (var tag in CborTags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException) { }
            catch (CborWrongTypeException) { }
        }
        throw new CborWrongTypeException();
    }

    // --- Display ---

    /// <inheritdoc/>
    public override string ToString() => $"SSKRShare({IdentifierHex()})";

    // --- Static SSKR operations ---

    /// <summary>
    /// Generates SSKR shares for the given spec and secret.
    /// </summary>
    /// <param name="spec">The split specification defining groups and thresholds.</param>
    /// <param name="secret">The secret to split into shares.</param>
    /// <returns>A list of groups, each containing <see cref="SSKRShare"/> instances.</returns>
    public static List<List<SSKRShare>> SskrGenerate(
        BlockchainCommons.SSKR.Spec spec,
        BlockchainCommons.SSKR.Secret secret)
    {
        return SskrGenerateUsing(spec, secret, SecureRandomNumberGenerator.Shared);
    }

    /// <summary>
    /// Generates SSKR shares using a custom random number generator.
    /// </summary>
    /// <param name="spec">The split specification defining groups and thresholds.</param>
    /// <param name="secret">The secret to split into shares.</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A list of groups, each containing <see cref="SSKRShare"/> instances.</returns>
    public static List<List<SSKRShare>> SskrGenerateUsing(
        BlockchainCommons.SSKR.Spec spec,
        BlockchainCommons.SSKR.Secret secret,
        IRandomNumberGenerator rng)
    {
        var rawShares = BlockchainCommons.SSKR.Sskr.GenerateUsing(spec, secret, rng);
        var result = new List<List<SSKRShare>>();
        foreach (var group in rawShares)
        {
            var groupShares = new List<SSKRShare>();
            foreach (var memberShares in group)
            {
                groupShares.Add(SSKRShare.FromData(memberShares));
            }
            result.Add(groupShares);
        }
        return result;
    }

    /// <summary>
    /// Combines SSKR shares to reconstruct the original secret.
    /// </summary>
    /// <param name="shares">The shares to combine.</param>
    /// <returns>The reconstructed secret.</returns>
    public static BlockchainCommons.SSKR.Secret SskrCombine(IReadOnlyList<SSKRShare> shares)
    {
        var shareData = new List<byte[]>();
        foreach (var share in shares)
        {
            shareData.Add(share.AsBytes());
        }
        return BlockchainCommons.SSKR.Sskr.Combine(shareData);
    }
}
