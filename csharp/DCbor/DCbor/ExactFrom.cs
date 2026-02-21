namespace BlockchainCommons.DCbor;

/// <summary>
/// Numeric exactness conversion helpers.
/// Returns null when the conversion would lose precision.
/// </summary>
internal static class ExactFrom
{
    // --- f64 -> integer ---

    internal static ulong? UInt64FromDouble(double source)
    {
        if (!double.IsFinite(source)) return null;
        if (source < 0.0 || source >= 18446744073709551616.0) return null;
        if (source != Math.Truncate(source)) return null;
        var result = (ulong)source;
        if ((double)result != source) return null;
        return result;
    }

    internal static long? Int64FromDouble(double source)
    {
        if (!double.IsFinite(source)) return null;
        if (source <= -9223372036854777856.0 || source >= 9223372036854775808.0) return null;
        if (source != Math.Truncate(source)) return null;
        var result = (long)source;
        if ((double)result != source) return null;
        return result;
    }

    // Int128 from double (for 65-bit negative handling)
    internal static Int128? Int128FromDouble(double source)
    {
        if (!double.IsFinite(source)) return null;
        if (source != Math.Truncate(source)) return null;
        // Int128 range is huge; we only use this for values near -2^64
        var result = (Int128)source;
        if ((double)result != source) return null;
        return result;
    }

    // UInt64 from Int128 (for extracting the negative CBOR value)
    internal static ulong? UInt64FromInt128(Int128 source)
    {
        if (source < 0 || source > (Int128)ulong.MaxValue) return null;
        return (ulong)source;
    }

    // --- f32 -> integer ---

    internal static uint? UInt32FromFloat(float source)
    {
        if (!float.IsFinite(source)) return null;
        if (source < 0.0f || source > 4294967295.0f) return null;
        if (source != MathF.Truncate(source)) return null;
        var result = (uint)source;
        if ((float)result != source) return null;
        return result;
    }

    internal static ulong? UInt64FromFloat(float source)
    {
        if (!float.IsFinite(source)) return null;
        if (source < 0.0f) return null;
        if (source != MathF.Truncate(source)) return null;
        var result = (ulong)source;
        if ((float)result != source) return null;
        return result;
    }

    // --- f64 -> f32 ---

    internal static float? Float32FromDouble(double source)
    {
        if (double.IsNaN(source)) return float.NaN;
        if (double.IsPositiveInfinity(source)) return float.PositiveInfinity;
        if (double.IsNegativeInfinity(source)) return float.NegativeInfinity;
        var f = (float)source;
        if ((double)f != source) return null;
        return f;
    }

    // --- f64 -> f64 exact from u64 ---

    internal static double? DoubleFromUInt64(ulong source)
    {
        var f = (double)source;
        if ((ulong)f != source) return null;
        return f;
    }
}
