namespace BlockchainCommons.DCbor;

/// <summary>
/// IEEE 754 half-precision (f16) encoding/decoding and canonical float encoding
/// for dCBOR. Handles NaN canonicalization, float-to-integer reduction, and
/// shortest-width encoding.
/// </summary>
internal static class FloatEncoding
{
    private static readonly byte[] CborNan = { 0xf9, 0x7e, 0x00 };

    // --- Half-precision (f16) helpers ---

    internal static double HalfToDouble(ushort bits)
    {
        int sign = (bits >> 15) & 1;
        int exponent = (bits >> 10) & 0x1f;
        int mantissa = bits & 0x3ff;

        double result;
        if (exponent == 0)
        {
            // Subnormal or zero
            result = Math.ScaleB(mantissa, -24);
        }
        else if (exponent == 31)
        {
            // Inf or NaN
            result = mantissa == 0 ? double.PositiveInfinity : double.NaN;
        }
        else
        {
            result = Math.ScaleB(mantissa + 1024, exponent - 25);
        }

        return sign == 1 ? -result : result;
    }

    internal static ushort DoubleToHalfBits(double value)
    {
        // We need to convert f64 -> f16 bits
        // This is only called when we know the round-trip is exact
        long bits = BitConverter.DoubleToInt64Bits(value);
        int sign = (int)((bits >> 63) & 1);
        int exp = (int)((bits >> 52) & 0x7ff);
        long frac = bits & 0xfffffffffffffL;

        if (exp == 0x7ff)
        {
            // Inf or NaN
            if (frac == 0)
                return (ushort)((sign << 15) | 0x7c00);
            else
                return 0x7e00; // canonical NaN
        }

        // Convert to half-precision
        // Unbias double exponent (1023) and rebias for half (15)
        double abs = Math.Abs(value);
        if (abs == 0.0)
            return (ushort)(sign << 15);

        // Use float32 as an intermediate to get the bits right
        float f = (float)value;
        uint fbits = BitConverter.SingleToUInt32Bits(f);
        int fexp = (int)((fbits >> 23) & 0xff);
        uint ffrac = fbits & 0x7fffff;

        int halfSign = sign << 15;

        if (fexp == 0xff)
        {
            if (ffrac == 0)
                return (ushort)(halfSign | 0x7c00);
            return 0x7e00;
        }

        // Rebias: float bias 127, half bias 15
        int halfExp = fexp - 127 + 15;

        if (halfExp >= 31)
            return (ushort)(halfSign | 0x7c00); // overflow to inf

        if (halfExp <= 0)
        {
            // Subnormal
            if (halfExp < -10)
                return (ushort)halfSign; // too small

            uint mant = ffrac | 0x800000;
            int shift = 14 - halfExp;
            ushort halfMant = (ushort)(mant >> shift);
            return (ushort)(halfSign | halfMant);
        }

        ushort halfFrac = (ushort)(ffrac >> 13);
        return (ushort)(halfSign | (halfExp << 10) | halfFrac);
    }

    internal static bool CanRoundTripAsHalf(double value)
    {
        if (double.IsNaN(value)) return true;
        if (double.IsInfinity(value)) return true;
        ushort bits = DoubleToHalfBits(value);
        return HalfToDouble(bits) == value;
    }

    internal static bool CanRoundTripAsHalf(float value)
    {
        if (float.IsNaN(value)) return true;
        if (float.IsInfinity(value)) return true;
        ushort bits = DoubleToHalfBits(value);
        return (float)HalfToDouble(bits) == value;
    }

    // --- Canonical float encoding ---

    /// <summary>
    /// Encode a double to canonical dCBOR bytes. Applies numeric reduction
    /// (float-to-int), NaN canonicalization, and shortest-width encoding.
    /// </summary>
    internal static byte[] DoubleToCborData(double value)
    {
        float f = (float)value;
        if ((double)f == value)
            return SingleToCborData(f);

        // Try integer reduction for negative values
        if (value < 0.0)
        {
            var n = ExactFrom.Int128FromDouble(value);
            if (n.HasValue)
            {
                var neg = -1 - n.Value;
                var i = ExactFrom.UInt64FromInt128(neg);
                if (i.HasValue)
                {
                    // Encode as Negative
                    return Varint.EncodeVarInt(i.Value, MajorType.Negative);
                }
            }
        }

        // Try unsigned integer reduction
        var ui = ExactFrom.UInt64FromDouble(value);
        if (ui.HasValue)
            return Varint.EncodeVarInt(ui.Value, MajorType.Unsigned);

        if (double.IsNaN(value))
            return (byte[])CborNan.Clone();

        // Encode as f64
        ulong bits = BitConverter.DoubleToUInt64Bits(value);
        return Varint.EncodeInt(bits, MajorType.Simple);
    }

    internal static byte[] SingleToCborData(float value)
    {
        if (CanRoundTripAsHalf(value))
            return HalfToCborData(value);

        // Try integer reduction for negative f32
        if (value < 0.0f)
        {
            var i = ExactFrom.UInt64FromFloat(-1f - value);
            if (i.HasValue)
                return Varint.EncodeVarInt(i.Value, MajorType.Negative);
        }

        // Try unsigned integer reduction
        var ui = ExactFrom.UInt32FromFloat(value);
        if (ui.HasValue)
            return Varint.EncodeVarInt(ui.Value, MajorType.Unsigned);

        if (float.IsNaN(value))
            return (byte[])CborNan.Clone();

        // Encode as f32
        uint bits = BitConverter.SingleToUInt32Bits(value);
        return Varint.EncodeInt(bits, MajorType.Simple);
    }

    internal static byte[] HalfToCborData(double value)
    {
        // Try integer reduction
        if (value < 0.0)
        {
            var i = ExactFrom.UInt64FromDouble(-1.0 - value);
            if (i.HasValue)
                return Varint.EncodeVarInt(i.Value, MajorType.Negative);
        }

        var ui = ExactFrom.UInt64FromDouble(value);
        if (ui.HasValue)
            return Varint.EncodeVarInt(ui.Value, MajorType.Unsigned);

        if (double.IsNaN(value))
            return (byte[])CborNan.Clone();

        ushort bits = DoubleToHalfBits(value);
        return Varint.EncodeInt(bits, MajorType.Simple);
    }

    // --- Canonical validation for decoding ---

    internal static void ValidateCanonicalF16(ushort bits)
    {
        double f = HalfToDouble(bits);
        bool isNan = double.IsNaN(f);
        if ((f == (double)(long)f && !isNan) || (isNan && bits != 0x7e00))
            throw new CborNonCanonicalNumericException();
    }

    internal static void ValidateCanonicalF32(float value)
    {
        if (CanRoundTripAsHalf(value) || value == (float)(int)value || float.IsNaN(value))
            throw new CborNonCanonicalNumericException();
    }

    internal static void ValidateCanonicalF64(double value)
    {
        if (value == (double)(float)value || value == (double)(long)value || double.IsNaN(value))
            throw new CborNonCanonicalNumericException();
    }
}
