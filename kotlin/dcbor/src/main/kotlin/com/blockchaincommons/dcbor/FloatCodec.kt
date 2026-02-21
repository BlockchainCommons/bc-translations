package com.blockchaincommons.dcbor

/**
 * Float encoding/decoding for dCBOR deterministic representation.
 *
 * Uses [Half] for IEEE 754 half-precision (f16) support.
 */
internal object FloatCodec {
    private val CBOR_NAN = byteArrayOf(0xf9.toByte(), 0x7e.toByte(), 0x00.toByte())

    // ---- f16 helpers using Half ----

    fun f16FromBits(bits: UShort): Float = Half.toFloat(bits)
    fun f16ToBits(f: Float): UShort = Half.fromFloat(f)
    fun f16FromFloat(f: Float): Float = Half.toFloat(Half.fromFloat(f))

    // ---- Encoding ----

    fun f64CborData(value: Double): ByteArray {
        val n = value
        val f = n.toFloat()
        if (f.toDouble() == n) {
            return f32CborData(f)
        }
        // Check negative integer reduction
        if (n < 0.0) {
            val i = Exact.longExactFromDouble(n)
            if (i != null && i < 0) {
                val neg = -1L - i
                if (neg >= 0) {
                    return Varint.encode(neg.toULong(), MajorType.Negative)
                }
            }
        }
        // Check positive integer reduction
        val u = Exact.ulongFromDouble(n)
        if (u != null) {
            return Varint.encode(u, MajorType.Unsigned)
        }
        if (value.isNaN()) {
            return CBOR_NAN.copyOf()
        }
        return Varint.encodeFixedInt64(java.lang.Double.doubleToRawLongBits(value).toULong(), MajorType.Simple)
    }

    fun f32CborData(value: Float): ByteArray {
        val n = value
        val f16 = f16FromFloat(n)
        if (f16 == n || (f16.isNaN() && n.isNaN())) {
            return f16CborData(f16ToBits(n))
        }
        // Check negative integer reduction
        if (n < 0.0f) {
            val neg = Exact.ulongFromFloat(-1.0f - n)
            if (neg != null) {
                return Varint.encode(neg, MajorType.Negative)
            }
        }
        // Check positive integer reduction
        val u = Exact.uintFromFloat(n)
        if (u != null) {
            return Varint.encode(u.toULong(), MajorType.Unsigned)
        }
        if (value.isNaN()) {
            return CBOR_NAN.copyOf()
        }
        return Varint.encodeFixedInt32(java.lang.Float.floatToRawIntBits(value).toUInt(), MajorType.Simple)
    }

    fun f16CborData(bits: UShort): ByteArray {
        val f = f16FromBits(bits)
        val n = f.toDouble()
        // Check negative integer reduction
        if (n < 0.0) {
            val u = Exact.ulongFromDouble(-1.0 - n)
            if (u != null) {
                return Varint.encode(u, MajorType.Negative)
            }
        }
        // Check positive integer reduction
        val u = Exact.ushortFromDouble(n)
        if (u != null) {
            return Varint.encode(u.toULong(), MajorType.Unsigned)
        }
        if (f.isNaN()) {
            return CBOR_NAN.copyOf()
        }
        return Varint.encodeFixedInt16(bits, MajorType.Simple)
    }

    // ---- Validation ----

    fun validateCanonicalF64(n: Double) {
        if (n == n.toFloat().toDouble() || n == n.toLong().toDouble() || n.isNaN()) {
            throw CborException.NonCanonicalNumeric()
        }
    }

    fun validateCanonicalF32(n: Float) {
        if (n == f16FromFloat(n) || n == n.toInt().toFloat() || n.isNaN()) {
            throw CborException.NonCanonicalNumeric()
        }
    }

    fun validateCanonicalF16(bits: UShort) {
        val f = f16FromBits(bits)
        val d = f.toDouble()
        if (d == d.toLong().toDouble() || (f.isNaN() && bits != 0x7e00u.toUShort())) {
            throw CborException.NonCanonicalNumeric()
        }
    }
}
