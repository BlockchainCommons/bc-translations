using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Fixed-size serialized state for the provenance-mark PRNG.
/// </summary>
public sealed class RngState : IEquatable<RngState>, ICborEncodable
{
    public const int Length = 32;

    private readonly byte[] _bytes;

    private RngState(byte[] bytes)
    {
        _bytes = bytes;
    }

    public static RngState FromBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length != Length)
        {
            throw ProvenanceMarkException.Cbor(
                $"invalid RNG state length: expected {Length} bytes, got {bytes.Length} bytes");
        }

        return new RngState((byte[])bytes.Clone());
    }

    public static RngState FromSlice(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != Length)
        {
            throw ProvenanceMarkException.Cbor(
                $"invalid RNG state length: expected {Length} bytes, got {bytes.Length} bytes");
        }

        return new RngState(bytes.ToArray());
    }

    public static RngState FromCbor(Cbor cbor)
    {
        try
        {
            return FromSlice(cbor.TryIntoByteString());
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Cbor(ex.Message, ex);
        }
    }

    public static RngState FromBase64(string value) => FromBytes(Util.FromBase64(value));

    public static RngState FromJson(string json)
    {
        try
        {
            var value = System.Text.Json.JsonSerializer.Deserialize<string>(json, Util.JsonOptions)
                ?? throw ProvenanceMarkException.Json("expected JSON string for RNG state");
            return FromBase64(value);
        }
        catch (ProvenanceMarkException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Json(ex.Message, ex);
        }
    }

    public byte[] ToBytes() => (byte[])_bytes.Clone();

    public string Hex => Util.ToHex(_bytes);

    public string ToBase64() => Util.ToBase64(_bytes);

    public string ToJson() => Util.SerializeJson(ToBase64());

    public Cbor ToCbor() => Cbor.ToByteString(_bytes);

    public bool Equals(RngState? other)
    {
        return other is not null && _bytes.AsSpan().SequenceEqual(other._bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as RngState);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _bytes)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }

    public override string ToString() => $"RngState({Hex})";
}
