package com.blockchaincommons.dcbor

/**
 * Exact numeric conversion utilities. These functions return null if the conversion would lose precision.
 */
internal object Exact {
    // ULong from Double
    fun ulongFromDouble(source: Double): ULong? {
        if (!source.isFinite()) return null
        if (source < 0.0 || source >= 18446744073709551616.0) return null
        if (source != kotlin.math.floor(source)) return null
        return source.toULong()
    }

    // ULong from Float
    fun ulongFromFloat(source: Float): ULong? {
        if (!source.isFinite()) return null
        if (source < 0.0f || source >= 18446744073709551616.0f) return null
        if (source != kotlin.math.floor(source)) return null
        return source.toULong()
    }

    // Long from Double
    fun longFromDouble(source: Double): Long? {
        if (!source.isFinite()) return null
        if (source <= -9223372036854777856.0 || source >= 9223372036854775808.0) return null
        if (source != kotlin.math.floor(source)) return null
        return source.toLong()
    }

    // Int from Double
    fun intFromDouble(source: Double): Int? {
        if (!source.isFinite()) return null
        if (source <= -2147483649.0 || source >= 2147483648.0) return null
        if (source != kotlin.math.floor(source)) return null
        return source.toInt()
    }

    // UInt from Float
    fun uintFromFloat(source: Float): UInt? {
        if (!source.isFinite()) return null
        if (source < 0.0f || source >= 4294967296.0f) return null
        if (source != kotlin.math.floor(source)) return null
        return source.toUInt()
    }

    // UShort from Double
    fun ushortFromDouble(source: Double): UShort? {
        if (!source.isFinite()) return null
        if (source < 0.0 || source >= 65536.0) return null
        if (source != kotlin.math.floor(source)) return null
        return source.toInt().toUShort()
    }

    // Double from ULong (exact)
    fun doubleFromULong(source: ULong): Double? {
        val f = source.toDouble()
        if (f.toULong() == source) return f
        return null
    }

    // Float from ULong (exact)
    fun floatFromULong(source: ULong): Float? {
        val f = source.toFloat()
        if (f.toULong() == source) return f
        return null
    }

    // Float from Double (exact)
    fun floatFromDouble(source: Double): Float? {
        if (source.isNaN()) return kotlin.Float.NaN
        val f = source.toFloat()
        if (f.toDouble() == source) return f
        return null
    }

    // Long (i128 equivalent) from Double — for negative integer reduction
    // In Kotlin, Long is 64-bit. For the negative path, we need to check
    // if a double represents an exact integer in the i128 range.
    // Since CBOR negative uses -1-n where n is u64, we only need Long range.
    fun longExactFromDouble(source: Double): Long? {
        if (!source.isFinite()) return null
        if (source != kotlin.math.floor(source)) return null
        // Check if within Long range
        if (source < Long.MIN_VALUE.toDouble() || source > Long.MAX_VALUE.toDouble()) {
            // For large values outside Long but still exact integer doubles
            return null
        }
        val l = source.toLong()
        if (l.toDouble() == source) return l
        return null
    }

    // Check if a ULong fits exactly in a Long
    fun longFromULong(source: ULong): Long? {
        if (source > Long.MAX_VALUE.toULong()) return null
        return source.toLong()
    }
}
