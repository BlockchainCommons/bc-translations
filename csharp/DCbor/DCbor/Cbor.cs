using System.Globalization;
using System.Text;

namespace BlockchainCommons.DCbor;

/// <summary>
/// The central immutable type representing a CBOR data item.
/// Wraps a <see cref="CborCase"/> and provides encoding, decoding,
/// equality, formatting, and convenience methods.
/// </summary>
public sealed class Cbor : IEquatable<Cbor>
{
    private readonly CborCase _case;

    public Cbor(CborCase cborCase)
    {
        _case = cborCase;
    }

    /// <summary>Returns the underlying discriminated-union case.</summary>
    public CborCase Case => _case;

    // --- Encoding ---

    /// <summary>Encodes this CBOR value to canonical dCBOR bytes.</summary>
    public byte[] ToCborData()
    {
        switch (_case)
        {
            case CborCase.UnsignedCase u:
                return Varint.EncodeVarInt(u.Value, MajorType.Unsigned);

            case CborCase.NegativeCase n:
                return Varint.EncodeVarInt(n.Value, MajorType.Negative);

            case CborCase.ByteStringCase bs:
            {
                var header = Varint.EncodeVarInt((ulong)bs.Value.Length, MajorType.ByteString);
                var result = new byte[header.Length + bs.Value.Length];
                header.CopyTo(result, 0);
                bs.Value.DataRef.CopyTo(result, header.Length);
                return result;
            }

            case CborCase.TextCase t:
            {
                string nfc = t.Value.IsNormalized(NormalizationForm.FormC)
                    ? t.Value
                    : t.Value.Normalize(NormalizationForm.FormC);
                byte[] utf8 = Encoding.UTF8.GetBytes(nfc);
                var header = Varint.EncodeVarInt((ulong)utf8.Length, MajorType.Text);
                var result = new byte[header.Length + utf8.Length];
                header.CopyTo(result, 0);
                utf8.CopyTo(result, header.Length);
                return result;
            }

            case CborCase.ArrayCase a:
            {
                var buf = new List<byte>(Varint.EncodeVarInt((ulong)a.Value.Count, MajorType.Array));
                foreach (var item in a.Value)
                    buf.AddRange(item.ToCborData());
                return buf.ToArray();
            }

            case CborCase.MapCase m:
                return m.Value.CborData();

            case CborCase.TaggedCase tg:
            {
                var header = Varint.EncodeVarInt(tg.Tag.Value, MajorType.Tagged);
                var itemData = tg.Item.ToCborData();
                var result = new byte[header.Length + itemData.Length];
                header.CopyTo(result, 0);
                itemData.CopyTo(result, header.Length);
                return result;
            }

            case CborCase.SimpleCase s:
                return s.Value.CborData();

            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>Returns the hex-encoded CBOR bytes.</summary>
    public string Hex() => Convert.ToHexString(ToCborData()).ToLowerInvariant();

    // --- Decoding ---

    /// <summary>Decodes binary data into a CBOR value, validating dCBOR rules.</summary>
    public static Cbor TryFromData(ReadOnlySpan<byte> data)
    {
        return CborDecoder.DecodeCbor(data);
    }

    /// <summary>Decodes a hex string into a CBOR value.</summary>
    public static Cbor TryFromHex(string hex)
    {
        byte[] data = Convert.FromHexString(hex);
        return TryFromData(data);
    }

    // --- From integer types ---

    public static Cbor FromInt(long value)
    {
        if (value < 0)
        {
            ulong n = (ulong)(-1 - (Int128)value);
            return new Cbor(CborCase.Negative(n));
        }
        return new Cbor(CborCase.Unsigned((ulong)value));
    }

    public static Cbor FromUInt(ulong value)
    {
        return new Cbor(CborCase.Unsigned(value));
    }

    // --- From float ---

    public static Cbor FromDouble(double value)
    {
        // Numeric reduction: try integer
        if (value < 0.0)
        {
            var n = ExactFrom.Int128FromDouble(value);
            if (n.HasValue)
            {
                var i = ExactFrom.UInt64FromInt128(-1 - n.Value);
                if (i.HasValue)
                    return new Cbor(CborCase.Negative(i.Value));
            }
        }

        var ui = ExactFrom.UInt64FromDouble(value);
        if (ui.HasValue)
            return new Cbor(CborCase.Unsigned(ui.Value));

        return new Cbor(CborCase.FromSimple(Simple.Float(value)));
    }

    public static Cbor FromFloat(float value)
    {
        if (value < 0.0f)
        {
            var i = ExactFrom.UInt64FromFloat(-1f - value);
            if (i.HasValue)
                return new Cbor(CborCase.Negative(i.Value));
        }

        var ui = ExactFrom.UInt32FromFloat(value);
        if (ui.HasValue)
            return new Cbor(CborCase.Unsigned(ui.Value));

        return new Cbor(CborCase.FromSimple(Simple.Float(value)));
    }

    // --- From other types ---

    public static Cbor FromString(string value) => new(CborCase.Text(value));
    public static Cbor FromBool(bool value) => new(CborCase.FromSimple(value ? Simple.True : Simple.False));
    public static Cbor FromByteString(byte[] data) => new(CborCase.FromByteString(new ByteString(data)));
    public static Cbor FromByteString(ByteString value) => new(CborCase.FromByteString(value));

    // --- Collection conversions ---

    /// <summary>Creates a CBOR array from a list of values.</summary>
    public static Cbor FromList(IEnumerable<Cbor> items)
    {
        return new Cbor(CborCase.Array(items.ToList()));
    }

    /// <summary>Creates a CBOR array from a list of integers.</summary>
    public static Cbor FromIntList(IEnumerable<int> items)
    {
        return FromList(items.Select(i => FromInt(i)));
    }

    /// <summary>Creates a CBOR array from a list of strings.</summary>
    public static Cbor FromStringList(IEnumerable<string> items)
    {
        return FromList(items.Select(s => FromString(s)));
    }

    /// <summary>Creates a CBOR map from a dictionary of int keys and string values.</summary>
    public static Cbor FromDictionary(IDictionary<int, string> dict)
    {
        var m = new CborMap();
        foreach (var kv in dict)
            m.Insert(FromInt(kv.Key), FromString(kv.Value));
        return new Cbor(CborCase.Map(m));
    }

    /// <summary>Extracts an array of typed values from a CBOR array.</summary>
    public List<int> TryIntoIntList()
    {
        var arr = TryIntoArray();
        return arr.Select(c => c.TryIntoInt32()).ToList();
    }

    /// <summary>Extracts a Dictionary from a CBOR map.</summary>
    public Dictionary<int, string> TryIntoDictionaryIntString()
    {
        var map = TryIntoMap();
        var result = new Dictionary<int, string>();
        foreach (var (key, value) in map)
            result[key.TryIntoInt32()] = value.TryIntoText();
        return result;
    }

    /// <summary>Extracts a SortedDictionary from a CBOR map.</summary>
    public SortedDictionary<int, string> TryIntoSortedDictionaryIntString()
    {
        var map = TryIntoMap();
        var result = new SortedDictionary<int, string>();
        foreach (var (key, value) in map)
            result[key.TryIntoInt32()] = value.TryIntoText();
        return result;
    }

    /// <summary>Extracts a HashSet of ints from a CBOR array.</summary>
    public HashSet<int> TryIntoHashSetInt()
    {
        var arr = TryIntoArray();
        return arr.Select(c => c.TryIntoInt32()).ToHashSet();
    }

    // --- Convenience factories ---

    public static Cbor Null() => new(CborCase.FromSimple(Simple.Null));
    public static Cbor True() => new(CborCase.FromSimple(Simple.True));
    public static Cbor False() => new(CborCase.FromSimple(Simple.False));
    public static Cbor Nan() => new(CborCase.FromSimple(Simple.Float(double.NaN)));

    public static Cbor ToByteString(byte[] data) => FromByteString(data);
    public static Cbor ToByteString(ReadOnlySpan<byte> data) => new(CborCase.FromByteString(new ByteString(data)));

    public static Cbor ToByteStringFromHex(string hex)
    {
        byte[] data = Convert.FromHexString(hex);
        return FromByteString(data);
    }

    public static Cbor ToTaggedValue(ulong tag, Cbor item)
    {
        return new Cbor(CborCase.Tagged(new Tag(tag), item));
    }

    public static Cbor ToTaggedValue(Tag tag, Cbor item)
    {
        return new Cbor(CborCase.Tagged(tag, item));
    }

    // --- Type inspection ---

    public bool IsNull => _case is CborCase.SimpleCase s && s.Value is Simple.NullValue;
    public bool IsTrue => _case is CborCase.SimpleCase s && s.Value is Simple.TrueValue;
    public bool IsFalse => _case is CborCase.SimpleCase s && s.Value is Simple.FalseValue;
    public bool IsBool => _case is CborCase.SimpleCase s && (s.Value is Simple.TrueValue || s.Value is Simple.FalseValue);
    public bool IsNumber => _case is CborCase.UnsignedCase or CborCase.NegativeCase or CborCase.SimpleCase { Value: Simple.FloatValue };
    public bool IsNan => _case is CborCase.SimpleCase s && s.Value.IsNaN;
    public bool IsText => _case is CborCase.TextCase;
    public bool IsByteString => _case is CborCase.ByteStringCase;
    public bool IsArray => _case is CborCase.ArrayCase;
    public bool IsMap => _case is CborCase.MapCase;
    public bool IsTaggedValue => _case is CborCase.TaggedCase;

    // --- Type extraction ---

    public bool? AsBool()
    {
        return _case switch
        {
            CborCase.SimpleCase { Value: Simple.TrueValue } => true,
            CborCase.SimpleCase { Value: Simple.FalseValue } => false,
            _ => null,
        };
    }

    public string? AsText()
    {
        return _case is CborCase.TextCase t ? t.Value : null;
    }

    public byte[]? AsByteStringData()
    {
        return _case is CborCase.ByteStringCase bs ? bs.Value.ToArray() : null;
    }

    public IReadOnlyList<Cbor>? AsArray()
    {
        return _case is CborCase.ArrayCase a ? a.Value : null;
    }

    public CborMap? AsMap()
    {
        return _case is CborCase.MapCase m ? m.Value : null;
    }

    public (Tag Tag, Cbor Item)? AsTaggedValue()
    {
        return _case is CborCase.TaggedCase tg ? (tg.Tag, tg.Item) : null;
    }

    // --- Try extraction (throwing) ---

    public bool TryIntoBool()
    {
        return AsBool() ?? throw new CborWrongTypeException();
    }

    public string TryIntoText()
    {
        return AsText() ?? throw new CborWrongTypeException();
    }

    public byte[] TryIntoByteString()
    {
        return AsByteStringData() ?? throw new CborWrongTypeException();
    }

    public IReadOnlyList<Cbor> TryIntoArray()
    {
        return AsArray() ?? throw new CborWrongTypeException();
    }

    public CborMap TryIntoMap()
    {
        return AsMap() ?? throw new CborWrongTypeException();
    }

    public (Tag Tag, Cbor Item) TryIntoTaggedValue()
    {
        return AsTaggedValue() ?? throw new CborWrongTypeException();
    }

    public Cbor TryIntoExpectedTaggedValue(Tag expectedTag)
    {
        var (tag, item) = TryIntoTaggedValue();
        if (tag != expectedTag)
            throw new CborWrongTagException(expectedTag, tag);
        return item;
    }

    public Cbor TryIntoExpectedTaggedValue(ulong expectedTagValue)
    {
        return TryIntoExpectedTaggedValue(new Tag(expectedTagValue));
    }

    // --- Numeric extraction ---

    public ulong TryIntoUInt64()
    {
        return _case switch
        {
            CborCase.UnsignedCase u => u.Value,
            _ => throw new CborWrongTypeException(),
        };
    }

    public long TryIntoInt64()
    {
        return _case switch
        {
            CborCase.UnsignedCase u when u.Value <= (ulong)long.MaxValue => (long)u.Value,
            CborCase.NegativeCase n when n.Value <= (ulong)long.MaxValue => -1L - (long)n.Value,
            CborCase.UnsignedCase or CborCase.NegativeCase => throw new CborOutOfRangeException(),
            _ => throw new CborWrongTypeException(),
        };
    }

    public int TryIntoInt32()
    {
        long v = TryIntoInt64();
        if (v < int.MinValue || v > int.MaxValue)
            throw new CborOutOfRangeException();
        return (int)v;
    }

    public double TryIntoDouble()
    {
        return _case switch
        {
            CborCase.UnsignedCase u =>
                ExactFrom.DoubleFromUInt64(u.Value) ?? throw new CborOutOfRangeException(),
            CborCase.NegativeCase n =>
                ExactFrom.DoubleFromUInt64(n.Value) is { } f
                    ? -1.0 - f
                    : throw new CborOutOfRangeException(),
            CborCase.SimpleCase { Value: Simple.FloatValue fv } => fv.Value,
            _ => throw new CborWrongTypeException(),
        };
    }

    // --- Equality ---

    public bool Equals(Cbor? other)
    {
        if (other is null) return false;
        return _case.Equals(other._case);
    }

    public override bool Equals(object? obj) => Equals(obj as Cbor);

    public override int GetHashCode() => _case.GetHashCode();

    public static bool operator ==(Cbor? a, Cbor? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(Cbor? a, Cbor? b) => !(a == b);

    // --- Display ---

    public override string ToString()
    {
        return _case switch
        {
            CborCase.UnsignedCase u => u.Value.ToString(),
            CborCase.NegativeCase n => (-1 - (Int128)n.Value).ToString(),
            CborCase.ByteStringCase bs => $"h'{bs.Value.ToHex()}'",
            CborCase.TextCase t => FormatString(t.Value),
            CborCase.ArrayCase a => FormatArray(a.Value),
            CborCase.MapCase m => m.Value.ToString()!,
            CborCase.TaggedCase tg => $"{tg.Tag}({tg.Item})",
            CborCase.SimpleCase s => s.Value.ToString()!,
            _ => throw new InvalidOperationException(),
        };
    }

    /// <summary>
    /// A verbose debug representation showing the internal structure.
    /// </summary>
    public string DebugDescription
    {
        get
        {
            return _case switch
            {
                CborCase.UnsignedCase u => $"unsigned({u.Value})",
                CborCase.NegativeCase n => $"negative({-1 - (Int128)n.Value})",
                CborCase.ByteStringCase bs => $"bytes({bs.Value.ToHex()})",
                CborCase.TextCase t => $"text(\"{t.Value}\")",
                CborCase.ArrayCase a => $"array([{string.Join(", ", a.Value.Select(x => x.DebugDescription))}])",
                CborCase.MapCase m => FormatMapDebug(m.Value),
                CborCase.TaggedCase tg => $"tagged({tg.Tag}, {tg.Item.DebugDescription})",
                CborCase.SimpleCase s => $"simple({s.Value.Name()})",
                _ => throw new InvalidOperationException(),
            };
        }
    }

    private static string FormatString(string s)
    {
        var sb = new StringBuilder();
        sb.Append('"');
        foreach (char c in s)
        {
            if (c == '"')
                sb.Append("\\\"");
            else
                sb.Append(c);
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static string FormatArray(IReadOnlyList<Cbor> a)
    {
        return "[" + string.Join(", ", a.Select(x => x.ToString())) + "]";
    }

    private static string FormatMapDebug(CborMap m)
    {
        var items = new List<string>();
        foreach (var (key, value) in m)
        {
            string keyHex = "0x" + Convert.ToHexString(key.ToCborData()).ToLowerInvariant();
            items.Add($"{keyHex}: ({key.DebugDescription}, {value.DebugDescription})");
        }
        return "map({" + string.Join(", ", items) + "})";
    }
}
