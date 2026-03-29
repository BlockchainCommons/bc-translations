using System.Buffers.Binary;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Encodes the space/precision tradeoffs for provenance mark serialization.
/// </summary>
public sealed class ProvenanceMarkResolution : IEquatable<ProvenanceMarkResolution>
{
    private ProvenanceMarkResolution(int code, string name, int linkLength, int seqBytesLength, int dateBytesLength)
    {
        Code = code;
        _name = name;
        _linkLength = linkLength;
        _seqBytesLength = seqBytesLength;
        _dateBytesLength = dateBytesLength;
    }

    private readonly string _name;
    private readonly int _linkLength;
    private readonly int _seqBytesLength;
    private readonly int _dateBytesLength;

    public int Code { get; }

    public static ProvenanceMarkResolution Low { get; } = new(0, "low", 4, 2, 2);
    public static ProvenanceMarkResolution Medium { get; } = new(1, "medium", 8, 4, 4);
    public static ProvenanceMarkResolution Quartile { get; } = new(2, "quartile", 16, 4, 6);
    public static ProvenanceMarkResolution High { get; } = new(3, "high", 32, 4, 6);

    public static IReadOnlyList<ProvenanceMarkResolution> All { get; } =
        [Low, Medium, Quartile, High];

    public int LinkLength() => _linkLength;

    public int SeqBytesLength() => _seqBytesLength;

    public int DateBytesLength() => _dateBytesLength;

    public int FixedLength() => (_linkLength * 3) + _seqBytesLength + _dateBytesLength;

    public Range KeyRange() => 0.._linkLength;

    public Range ChainIdRange() => 0.._linkLength;

    public Range HashRange()
    {
        var start = ChainIdRange().End.Value;
        return start..(start + _linkLength);
    }

    public Range SeqBytesRange()
    {
        var start = HashRange().End.Value;
        return start..(start + _seqBytesLength);
    }

    public Range DateBytesRange()
    {
        var start = SeqBytesRange().End.Value;
        return start..(start + _dateBytesLength);
    }

    public int InfoRangeStart() => DateBytesRange().End.Value;

    public byte[] SerializeDate(CborDate date)
    {
        return this switch
        {
            _ when Equals(Low) => DateSerialization.Serialize2Bytes(date),
            _ when Equals(Medium) => DateSerialization.Serialize4Bytes(date),
            _ when Equals(Quartile) || Equals(High) => DateSerialization.Serialize6Bytes(date),
            _ => throw ProvenanceMarkException.ResolutionError($"unsupported resolution value: {Code}")
        };
    }

    public CborDate DeserializeDate(ReadOnlySpan<byte> data)
    {
        if (Equals(Low) && data.Length == 2)
        {
            return DateSerialization.Deserialize2Bytes(data);
        }

        if (Equals(Medium) && data.Length == 4)
        {
            return DateSerialization.Deserialize4Bytes(data);
        }

        if ((Equals(Quartile) || Equals(High)) && data.Length == 6)
        {
            return DateSerialization.Deserialize6Bytes(data);
        }

        throw ProvenanceMarkException.ResolutionError(
            $"invalid date length: expected 2, 4, or 6 bytes, got {data.Length}");
    }

    public byte[] SerializeSeq(uint sequence)
    {
        return _seqBytesLength switch
        {
            2 when sequence > ushort.MaxValue => throw ProvenanceMarkException.ResolutionError(
                $"sequence number {sequence} out of range for 2-byte format (max {ushort.MaxValue})"),
            2 =>
            [
                (byte)((sequence >> 8) & 0xff),
                (byte)(sequence & 0xff)
            ],
            4 => BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(sequence)),
            _ => throw new InvalidOperationException("unsupported sequence byte length")
        };
    }

    public uint DeserializeSeq(ReadOnlySpan<byte> data)
    {
        return _seqBytesLength switch
        {
            2 when data.Length != 2 => throw ProvenanceMarkException.ResolutionError(
                $"invalid sequence number length: expected 2 or 4 bytes, got {data.Length}"),
            2 => (uint)((data[0] << 8) | data[1]),
            4 when data.Length != 4 => throw ProvenanceMarkException.ResolutionError(
                $"invalid sequence number length: expected 2 or 4 bytes, got {data.Length}"),
            4 => BinaryPrimitives.ReadUInt32BigEndian(data),
            _ => throw new InvalidOperationException("unsupported sequence byte length")
        };
    }

    public Cbor ToCbor() => Cbor.FromInt(Code);

    public static ProvenanceMarkResolution FromCode(int value)
    {
        return value switch
        {
            0 => Low,
            1 => Medium,
            2 => Quartile,
            3 => High,
            _ => throw ProvenanceMarkException.ResolutionError(
                $"invalid provenance mark resolution value: {value}")
        };
    }

    public static ProvenanceMarkResolution FromCbor(Cbor cbor)
    {
        try
        {
            return FromCode(cbor.TryIntoInt32());
        }
        catch (Exception ex) when (ex is CborException or InvalidOperationException)
        {
            throw ProvenanceMarkException.Cbor(ex.Message, ex);
        }
    }

    public bool Equals(ProvenanceMarkResolution? other)
    {
        return other is not null && Code == other.Code;
    }

    public override bool Equals(object? obj) => Equals(obj as ProvenanceMarkResolution);

    public override int GetHashCode() => Code;

    public static bool operator ==(ProvenanceMarkResolution? left, ProvenanceMarkResolution? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ProvenanceMarkResolution? left, ProvenanceMarkResolution? right) =>
        !(left == right);

    public override string ToString() => _name;
}
