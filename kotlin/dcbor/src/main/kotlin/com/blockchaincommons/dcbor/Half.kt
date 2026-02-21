package com.blockchaincommons.dcbor

/**
 * IEEE 754 half-precision (float16) support.
 *
 * Since JDK Float.float16ToFloat() requires JDK 20+, this provides
 * a manual implementation of f16 ↔ f32 conversion.
 */
internal object Half {
    const val NAN_BITS: UShort = 0x7E00u
    const val POS_INF_BITS: UShort = 0x7C00u
    const val NEG_INF_BITS: UShort = 0xFC00u

    /** Convert half-precision bits to Float. */
    fun toFloat(bits: UShort): Float {
        val h = bits.toInt()
        val sign = (h shr 15) and 1
        val exp = (h shr 10) and 0x1F
        val mant = h and 0x3FF

        return when {
            exp == 0 -> {
                // Subnormal or zero
                val f = (mant.toFloat() / 1024.0f) * (1.0f / 32768.0f) // 2^-14 * (mant/1024)
                if (sign == 1) -f else f
            }
            exp == 31 -> {
                // Infinity or NaN
                if (mant == 0) {
                    if (sign == 1) Float.NEGATIVE_INFINITY else Float.POSITIVE_INFINITY
                } else {
                    Float.NaN
                }
            }
            else -> {
                // Normal number
                val f = Math.scalb((1.0f + mant.toFloat() / 1024.0f), exp - 15)
                if (sign == 1) -f else f
            }
        }
    }

    /** Convert Float to half-precision bits. */
    fun fromFloat(f: Float): UShort {
        val bits = java.lang.Float.floatToRawIntBits(f)
        val sign = (bits ushr 31) and 1
        val exp = (bits ushr 23) and 0xFF
        val mant = bits and 0x7FFFFF

        val h: Int = when {
            exp == 0xFF -> {
                // Infinity or NaN
                if (mant != 0) {
                    // NaN — canonical
                    (sign shl 15) or 0x7E00
                } else {
                    // Infinity
                    (sign shl 15) or 0x7C00
                }
            }
            exp == 0 -> {
                // Zero or subnormal f32 → zero in f16
                sign shl 15
            }
            else -> {
                val unbiasedExp = exp - 127
                if (unbiasedExp > 15) {
                    // Overflow → infinity
                    (sign shl 15) or 0x7C00
                } else if (unbiasedExp < -14) {
                    // Underflow → subnormal or zero
                    val shift = -14 - unbiasedExp
                    val m = (mant or 0x800000) shr (13 + shift)
                    // Round
                    val roundBit = ((mant or 0x800000) shr (12 + shift)) and 1
                    (sign shl 15) or (m + roundBit)
                } else if (unbiasedExp == -14) {
                    // May be subnormal in f16
                    val m = (mant or 0x800000) shr 13
                    val roundBit = (mant shr 12) and 1
                    (sign shl 15) or (m + roundBit)
                } else {
                    // Normal
                    val halfExp = unbiasedExp + 15
                    val halfMant = mant shr 13
                    // Round
                    val roundBit = (mant shr 12) and 1
                    val result = (sign shl 15) or (halfExp shl 10) or (halfMant + roundBit)
                    // Handle mantissa overflow from rounding
                    result
                }
            }
        }
        return h.toUShort()
    }

    /** Convert half-precision bits to Double. */
    fun toDouble(bits: UShort): Double = toFloat(bits).toDouble()

    /** Check if half-precision bits represent NaN. */
    fun isNaN(bits: UShort): Boolean {
        val exp = (bits.toInt() shr 10) and 0x1F
        val mant = bits.toInt() and 0x3FF
        return exp == 31 && mant != 0
    }

    /** Check if half-precision bits represent infinity. */
    fun isInfinite(bits: UShort): Boolean {
        val exp = (bits.toInt() shr 10) and 0x1F
        val mant = bits.toInt() and 0x3FF
        return exp == 31 && mant == 0
    }

    /** Check if a Float can be exactly represented as f16. */
    fun canRepresentExactly(f: Float): Boolean {
        if (f.isNaN()) return true
        val bits = fromFloat(f)
        return toFloat(bits) == f
    }

    /** Max finite f16 value. */
    val MAX_VALUE: Float = toFloat(0x7BFFu)

    /** Min finite f16 value (most negative). */
    val MIN_VALUE: Float = toFloat(0xFBFFu)
}
