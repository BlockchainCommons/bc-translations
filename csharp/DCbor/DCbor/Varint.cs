namespace BlockchainCommons.DCbor;

/// <summary>
/// CBOR major types (3-bit field in the initial byte).
/// </summary>
public enum MajorType
{
    Unsigned = 0,
    Negative = 1,
    ByteString = 2,
    Text = 3,
    Array = 4,
    Map = 5,
    Tagged = 6,
    Simple = 7,
}

/// <summary>
/// Variable-length integer encoding for CBOR header bytes.
/// </summary>
internal static class Varint
{
    private static byte TypeBits(MajorType t) => (byte)((int)t << 5);

    /// <summary>
    /// Encodes a value using the shortest CBOR varint representation.
    /// </summary>
    public static byte[] EncodeVarInt(ulong value, MajorType majorType)
    {
        if (value <= 23)
            return new byte[] { (byte)(value | TypeBits(majorType)) };
        if (value <= byte.MaxValue)
            return EncodeInt((byte)value, majorType);
        if (value <= ushort.MaxValue)
            return EncodeInt((ushort)value, majorType);
        if (value <= uint.MaxValue)
            return EncodeInt((uint)value, majorType);
        return EncodeInt(value, majorType);
    }

    /// <summary>
    /// Encodes as a 1-byte payload (additional info 24).
    /// </summary>
    public static byte[] EncodeInt(byte value, MajorType majorType)
    {
        return new byte[] { (byte)(0x18 | TypeBits(majorType)), value };
    }

    /// <summary>
    /// Encodes as a 2-byte payload (additional info 25).
    /// </summary>
    public static byte[] EncodeInt(ushort value, MajorType majorType)
    {
        return new byte[]
        {
            (byte)(0x19 | TypeBits(majorType)),
            (byte)(value >> 8),
            (byte)value,
        };
    }

    /// <summary>
    /// Encodes as a 4-byte payload (additional info 26).
    /// </summary>
    public static byte[] EncodeInt(uint value, MajorType majorType)
    {
        return new byte[]
        {
            (byte)(0x1a | TypeBits(majorType)),
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value,
        };
    }

    /// <summary>
    /// Encodes as an 8-byte payload (additional info 27).
    /// </summary>
    public static byte[] EncodeInt(ulong value, MajorType majorType)
    {
        return new byte[]
        {
            (byte)(0x1b | TypeBits(majorType)),
            (byte)(value >> 56),
            (byte)(value >> 48),
            (byte)(value >> 40),
            (byte)(value >> 32),
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value,
        };
    }
}
