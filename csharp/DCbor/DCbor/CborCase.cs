namespace BlockchainCommons.DCbor;

/// <summary>
/// Discriminated union representing all possible CBOR data types (major types 0-7).
/// This is the core variant type wrapped by <see cref="Cbor"/>.
/// </summary>
public abstract class CborCase : IEquatable<CborCase>
{
    private CborCase() { }

    /// <summary>Major type 0: non-negative integer 0..2^64-1.</summary>
    public sealed class UnsignedCase : CborCase
    {
        public ulong Value { get; }
        public UnsignedCase(ulong value) { Value = value; }
        public override string ToString() => Value.ToString();
    }

    /// <summary>Major type 1: negative integer. Actual value is -1 - Value.</summary>
    public sealed class NegativeCase : CborCase
    {
        public ulong Value { get; }
        public NegativeCase(ulong value) { Value = value; }
        public override string ToString() => (-1 - (Int128)Value).ToString();
    }

    /// <summary>Major type 2: byte string.</summary>
    public sealed class ByteStringCase : CborCase
    {
        public ByteString Value { get; }
        public ByteStringCase(ByteString value) { Value = value; }
        public override string ToString() => $"h'{Value.ToHex()}'";
    }

    /// <summary>Major type 3: UTF-8 text string.</summary>
    public sealed class TextCase : CborCase
    {
        public string Value { get; }
        public TextCase(string value) { Value = value; }
        public override string ToString() => $"\"{Value}\"";
    }

    /// <summary>Major type 4: ordered array of CBOR items.</summary>
    public sealed class ArrayCase : CborCase
    {
        public IReadOnlyList<Cbor> Value { get; }
        public ArrayCase(IReadOnlyList<Cbor> value) { Value = value; }
    }

    /// <summary>Major type 5: map of CBOR key-value pairs.</summary>
    public sealed class MapCase : CborCase
    {
        public CborMap Value { get; }
        public MapCase(CborMap value) { Value = value; }
    }

    /// <summary>Major type 6: tagged value.</summary>
    public sealed class TaggedCase : CborCase
    {
        public Tag Tag { get; }
        public Cbor Item { get; }
        public TaggedCase(Tag tag, Cbor item) { Tag = tag; Item = item; }
    }

    /// <summary>Major type 7: simple values (false, true, null, float).</summary>
    public sealed class SimpleCase : CborCase
    {
        public Simple Value { get; }
        public SimpleCase(Simple value) { Value = value; }
    }

    // --- Factory methods ---

    public static CborCase Unsigned(ulong value) => new UnsignedCase(value);
    public static CborCase Negative(ulong value) => new NegativeCase(value);
    public static CborCase FromByteString(ByteString value) => new ByteStringCase(value);
    public static CborCase Text(string value) => new TextCase(value);
    public static CborCase Array(IReadOnlyList<Cbor> value) => new ArrayCase(value);
    public static CborCase Map(CborMap value) => new MapCase(value);
    public static CborCase Tagged(Tag tag, Cbor item) => new TaggedCase(tag, item);
    public static CborCase FromSimple(Simple value) => new SimpleCase(value);

    // --- Equality ---

    public bool Equals(CborCase? other)
    {
        if (other is null) return false;
        return (this, other) switch
        {
            (UnsignedCase a, UnsignedCase b) => a.Value == b.Value,
            (NegativeCase a, NegativeCase b) => a.Value == b.Value,
            (ByteStringCase a, ByteStringCase b) => a.Value.Equals(b.Value),
            (TextCase a, TextCase b) => a.Value == b.Value,
            (ArrayCase a, ArrayCase b) => a.Value.SequenceEqual(b.Value),
            (MapCase a, MapCase b) => a.Value.Equals(b.Value),
            (TaggedCase a, TaggedCase b) => a.Tag.Equals(b.Tag) && a.Item.Equals(b.Item),
            (SimpleCase a, SimpleCase b) => a.Value.Equals(b.Value),
            _ => false,
        };
    }

    public override bool Equals(object? obj) => Equals(obj as CborCase);

    public override int GetHashCode()
    {
        return this switch
        {
            UnsignedCase a => HashCode.Combine(0, a.Value),
            NegativeCase a => HashCode.Combine(1, a.Value),
            ByteStringCase a => HashCode.Combine(2, a.Value),
            TextCase a => HashCode.Combine(3, a.Value),
            ArrayCase a => HashCode.Combine(4, a.Value.Count),
            MapCase a => HashCode.Combine(5, a.Value.Count),
            TaggedCase a => HashCode.Combine(6, a.Tag, a.Item),
            SimpleCase a => HashCode.Combine(7, a.Value),
            _ => 0,
        };
    }
}
