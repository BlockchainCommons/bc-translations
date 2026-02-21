namespace BlockchainCommons.DCbor;

/// <summary>
/// Represents CBOR simple values (major type 7): false, true, null, and float.
/// </summary>
public abstract class Simple : IEquatable<Simple>
{
    private Simple() { }

    public sealed class FalseValue : Simple
    {
        internal static readonly FalseValue Instance = new();
        public override string ToString() => "false";
    }

    public sealed class TrueValue : Simple
    {
        internal static readonly TrueValue Instance = new();
        public override string ToString() => "true";
    }

    public sealed class NullValue : Simple
    {
        internal static readonly NullValue Instance = new();
        public override string ToString() => "null";
    }

    public sealed class FloatValue : Simple
    {
        public double Value { get; }
        public FloatValue(double value) { Value = value; }

        public override string ToString()
        {
            if (double.IsNaN(Value)) return "NaN";
            if (double.IsPositiveInfinity(Value)) return "Infinity";
            if (double.IsNegativeInfinity(Value)) return "-Infinity";
            return FormatDouble(Value);
        }
    }

    public static Simple False => FalseValue.Instance;
    public static Simple True => TrueValue.Instance;
    public static Simple Null => NullValue.Instance;
    public static Simple Float(double value) => new FloatValue(value);

    public bool IsFloat => this is FloatValue;
    public bool IsNaN => this is FloatValue fv && double.IsNaN(fv.Value);

    /// <summary>
    /// Returns the debug/name representation (used for Debug formatting).
    /// </summary>
    public string Name()
    {
        return this switch
        {
            FalseValue => "false",
            TrueValue => "true",
            NullValue => "null",
            FloatValue fv => FormatDouble(fv.Value),
            _ => throw new InvalidOperationException()
        };
    }

    /// <summary>
    /// Encodes this simple value to CBOR bytes.
    /// </summary>
    public byte[] CborData()
    {
        return this switch
        {
            FalseValue => Varint.EncodeVarInt(20, MajorType.Simple),
            TrueValue => Varint.EncodeVarInt(21, MajorType.Simple),
            NullValue => Varint.EncodeVarInt(22, MajorType.Simple),
            FloatValue fv => FloatEncoding.DoubleToCborData(fv.Value),
            _ => throw new InvalidOperationException()
        };
    }

    // --- Equality ---

    public bool Equals(Simple? other)
    {
        if (other is null) return false;
        return (this, other) switch
        {
            (FalseValue, FalseValue) => true,
            (TrueValue, TrueValue) => true,
            (NullValue, NullValue) => true,
            (FloatValue a, FloatValue b) => a.Value == b.Value || (double.IsNaN(a.Value) && double.IsNaN(b.Value)),
            _ => false
        };
    }

    public override bool Equals(object? obj) => Equals(obj as Simple);

    public override int GetHashCode()
    {
        return this switch
        {
            FalseValue => HashCode.Combine(0),
            TrueValue => HashCode.Combine(1),
            NullValue => HashCode.Combine(2),
            FloatValue fv => HashCode.Combine(BitConverter.DoubleToInt64Bits(fv.Value)),
            _ => 0
        };
    }

    // --- Double formatting for canonical display ---

    internal static string FormatDouble(double v)
    {
        if (double.IsNaN(v)) return "NaN";
        if (double.IsPositiveInfinity(v)) return "inf";
        if (double.IsNegativeInfinity(v)) return "-inf";

        // Format with round-trip precision, always including a decimal point
        string s = v.ToString("R");
        // If the result doesn't contain a '.' or 'E', add ".0"
        if (!s.Contains('.') && !s.Contains('E') && !s.Contains('e'))
        {
            s += ".0";
        }
        // Normalize scientific notation: lowercase 'e', no '+' sign, minimal exponent digits
        s = s.Replace('E', 'e');
        s = s.Replace("e+", "e");
        int eIdx = s.IndexOf('e');
        if (eIdx >= 0)
        {
            string mantissa = s[..eIdx];
            string expPart = s[(eIdx + 1)..];
            bool negExp = expPart.StartsWith('-');
            if (negExp) expPart = expPart[1..];
            expPart = expPart.TrimStart('0');
            if (expPart.Length == 0) expPart = "0";
            s = mantissa + "e" + (negExp ? "-" : "") + expPart;
        }
        return s;
    }
}
