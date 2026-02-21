using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR;

/// <summary>
/// A part emitted by a fountain encoder, consisting of fragment metadata and XOR-combined data.
/// </summary>
internal sealed class FountainPart
{
    internal int Sequence { get; }
    internal int SequenceCount { get; }
    internal int MessageLength { get; }
    internal uint Checksum { get; }
    internal byte[] Data { get; }

    internal FountainPart(int sequence, int sequenceCount, int messageLength, uint checksum, byte[] data)
    {
        Sequence = sequence;
        SequenceCount = sequenceCount;
        MessageLength = messageLength;
        Checksum = checksum;
        Data = data;
    }

    /// <summary>
    /// Returns the indexes of the message segments combined into this part.
    /// </summary>
    internal List<int> Indexes()
    {
        return FountainUtils.ChooseFragments(Sequence, SequenceCount, Checksum);
    }

    /// <summary>
    /// Whether this part represents a single original segment (not a combination).
    /// </summary>
    internal bool IsSimple => Indexes().Count == 1;

    /// <summary>
    /// Returns the sequence ID string "seq-count" for UR encoding.
    /// </summary>
    internal string SequenceId => $"{Sequence}-{SequenceCount}";

    /// <summary>
    /// Encodes this part as CBOR: a 5-element array [sequence, sequenceCount, messageLength, checksum, data].
    /// Uses raw CBOR encoding matching minicbor's format (u32 integers, not u64).
    /// </summary>
    internal byte[] ToCbor()
    {
        // Build CBOR manually to match minicbor's exact output format:
        // array(5) + u32 + u32 + u32 + u32 + bstr
        using var ms = new MemoryStream();

        // CBOR array header: major type 4, additional info 5
        ms.WriteByte(0x85);

        // Four unsigned integers (encoded as CBOR unsigned)
        WriteCborUInt(ms, (uint)Sequence);
        WriteCborUInt(ms, (uint)SequenceCount);
        WriteCborUInt(ms, (uint)MessageLength);
        WriteCborUInt(ms, Checksum);

        // Byte string
        WriteCborBytes(ms, Data);

        return ms.ToArray();
    }

    /// <summary>
    /// Decodes a FountainPart from CBOR bytes.
    /// </summary>
    internal static FountainPart FromCbor(byte[] cbor)
    {
        try
        {
            var decoded = Cbor.TryFromData(cbor);
            var array = decoded.TryIntoArray();
            if (array.Count != 5)
            {
                throw new FountainException("invalid CBOR array length");
            }

            var sequence = (int)array[0].TryIntoUInt64();
            var sequenceCount = (int)array[1].TryIntoUInt64();
            var messageLength = (int)array[2].TryIntoUInt64();
            var checksum = (uint)array[3].TryIntoUInt64();
            var data = array[4].TryIntoByteString();

            return new FountainPart(sequence, sequenceCount, messageLength, checksum, data);
        }
        catch (Exception ex) when (ex is not FountainException)
        {
            throw new FountainException(ex.Message);
        }
    }

    private static void WriteCborUInt(MemoryStream ms, uint value)
    {
        if (value <= 23)
        {
            ms.WriteByte((byte)value);
        }
        else if (value <= 0xFF)
        {
            ms.WriteByte(0x18);
            ms.WriteByte((byte)value);
        }
        else if (value <= 0xFFFF)
        {
            ms.WriteByte(0x19);
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)value);
        }
        else
        {
            ms.WriteByte(0x1A);
            ms.WriteByte((byte)(value >> 24));
            ms.WriteByte((byte)(value >> 16));
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)value);
        }
    }

    private static void WriteCborBytes(MemoryStream ms, byte[] data)
    {
        var len = (uint)data.Length;
        if (len <= 23)
        {
            ms.WriteByte((byte)(0x40 | len));
        }
        else if (len <= 0xFF)
        {
            ms.WriteByte(0x58);
            ms.WriteByte((byte)len);
        }
        else if (len <= 0xFFFF)
        {
            ms.WriteByte(0x59);
            ms.WriteByte((byte)(len >> 8));
            ms.WriteByte((byte)len);
        }
        else
        {
            ms.WriteByte(0x5A);
            ms.WriteByte((byte)(len >> 24));
            ms.WriteByte((byte)(len >> 16));
            ms.WriteByte((byte)(len >> 8));
            ms.WriteByte((byte)len);
        }
        ms.Write(data, 0, data.Length);
    }

    internal FountainPart Clone()
    {
        return new FountainPart(Sequence, SequenceCount, MessageLength, Checksum, (byte[])Data.Clone());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FountainPart other) return false;
        return Sequence == other.Sequence
            && SequenceCount == other.SequenceCount
            && MessageLength == other.MessageLength
            && Checksum == other.Checksum
            && Data.AsSpan().SequenceEqual(other.Data);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Sequence, SequenceCount, MessageLength, Checksum);
    }
}
