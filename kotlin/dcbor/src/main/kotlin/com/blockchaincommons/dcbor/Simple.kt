package com.blockchaincommons.dcbor

/**
 * Represents CBOR simple values (major type 7).
 *
 * In dCBOR, only false, true, null, and floating point values are valid.
 */
sealed class Simple {
    data object False : Simple()
    data object True : Simple()
    data object Null : Simple()
    data class Float(val value: Double) : Simple() {
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is Float) return false
            return value == other.value || (value.isNaN() && other.value.isNaN())
        }

        override fun hashCode(): Int = java.lang.Double.doubleToRawLongBits(value).hashCode()
    }

    fun isFloat(): Boolean = this is Float
    fun isNaN(): Boolean = this is Float && value.isNaN()

    val name: String get() = when (this) {
        is False -> "false"
        is True -> "true"
        is Null -> "null"
        is Float -> value.toString()
    }

    fun toCborData(): ByteArray = when (this) {
        is False -> Varint.encode(20u, MajorType.Simple)
        is True -> Varint.encode(21u, MajorType.Simple)
        is Null -> Varint.encode(22u, MajorType.Simple)
        is Float -> FloatCodec.f64CborData(value)
    }

    val debugDescription: String get() = when (this) {
        is False -> "false"
        is True -> "true"
        is Null -> "null"
        is Float -> formatDouble(value)
    }

    val displayDescription: String get() = when (this) {
        is False -> "false"
        is True -> "true"
        is Null -> "null"
        is Float -> when {
            value.isNaN() -> "NaN"
            value.isInfinite() -> if (value > 0) "Infinity" else "-Infinity"
            else -> formatDouble(value)
        }
    }

    companion object {
        /** Formats a double with minimal decimal notation, preserving at least one decimal place. */
        internal fun formatDouble(v: Double): String {
            if (v.isNaN()) return "NaN"
            if (v.isInfinite()) return if (v > 0) "inf" else "-inf"
            val s = v.toBigDecimal().stripTrailingZeros().toPlainString()
            // Ensure at least one decimal point
            return if ('.' in s) s else "$s.0"
        }
    }
}
