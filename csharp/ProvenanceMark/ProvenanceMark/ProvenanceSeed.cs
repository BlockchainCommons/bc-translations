using BlockchainCommons.BCRand;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Fixed-size 32-byte provenance seed.
/// </summary>
public sealed class ProvenanceSeed : IEquatable<ProvenanceSeed>, ICborEncodable
{
    public const int Length = 32;

    private readonly byte[] _bytes;

    private ProvenanceSeed(byte[] bytes)
    {
        _bytes = bytes;
    }

    public static ProvenanceSeed Create()
    {
        return CreateUsing(RandomNumberGeneratorExtensions.ThreadRng());
    }

    public static ProvenanceSeed CreateUsing(IRandomNumberGenerator rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        return FromBytes(rng.RandomData(Length));
    }

    public static ProvenanceSeed CreateWithPassphrase(string passphrase)
    {
        ArgumentNullException.ThrowIfNull(passphrase);
        return FromBytes(CryptoUtils.ExtendKey(System.Text.Encoding.UTF8.GetBytes(passphrase)));
    }

    public static ProvenanceSeed FromBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length != Length)
        {
            throw ProvenanceMarkException.InvalidSeedLength(bytes.Length);
        }

        return new ProvenanceSeed((byte[])bytes.Clone());
    }

    public static ProvenanceSeed FromSlice(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != Length)
        {
            throw ProvenanceMarkException.InvalidSeedLength(bytes.Length);
        }

        return new ProvenanceSeed(bytes.ToArray());
    }

    public static ProvenanceSeed FromCbor(Cbor cbor)
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

    public static ProvenanceSeed FromBase64(string value) => FromBytes(Util.FromBase64(value));

    public static ProvenanceSeed FromJson(string json)
    {
        try
        {
            var value = System.Text.Json.JsonSerializer.Deserialize<string>(json, Util.JsonOptions)
                ?? throw ProvenanceMarkException.Json("expected JSON string for provenance seed");
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

    public bool Equals(ProvenanceSeed? other)
    {
        return other is not null && _bytes.AsSpan().SequenceEqual(other._bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as ProvenanceSeed);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _bytes)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }

    public override string ToString() => Hex;
}
