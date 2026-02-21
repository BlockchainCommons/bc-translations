package com.blockchaincommons.dcbor

internal enum class MajorType(val value: Int) {
    Unsigned(0),
    Negative(1),
    ByteString(2),
    Text(3),
    Array(4),
    Map(5),
    Tagged(6),
    Simple(7);

    fun typeBits(): UByte = (value shl 5).toUByte()
}

internal object Varint {
    fun encode(n: ULong, majorType: MajorType): ByteArray {
        return when {
            n <= 23u.toULong() -> byteArrayOf((n.toUByte() or majorType.typeBits()).toByte())
            n <= UByte.MAX_VALUE.toULong() -> encodeInt8(n.toUByte(), majorType)
            n <= UShort.MAX_VALUE.toULong() -> encodeInt16(n.toUShort(), majorType)
            n <= UInt.MAX_VALUE.toULong() -> encodeInt32(n.toUInt(), majorType)
            else -> encodeInt64(n, majorType)
        }
    }

    fun encodeInt8(n: UByte, majorType: MajorType): ByteArray {
        return byteArrayOf(
            (0x18u.toUByte() or majorType.typeBits()).toByte(),
            n.toByte()
        )
    }

    fun encodeInt16(n: UShort, majorType: MajorType): ByteArray {
        val v = n.toInt()
        return if (v <= UByte.MAX_VALUE.toInt()) {
            encode(v.toULong(), majorType)
        } else {
            byteArrayOf(
                (0x19u.toUByte() or majorType.typeBits()).toByte(),
                (v shr 8).toByte(),
                v.toByte()
            )
        }
    }

    fun encodeInt32(n: UInt, majorType: MajorType): ByteArray {
        val v = n.toLong()
        return if (v <= UShort.MAX_VALUE.toLong()) {
            encode(v.toULong(), majorType)
        } else {
            byteArrayOf(
                (0x1au.toUByte() or majorType.typeBits()).toByte(),
                (v shr 24).toByte(),
                (v shr 16).toByte(),
                (v shr 8).toByte(),
                v.toByte()
            )
        }
    }

    fun encodeInt64(n: ULong, majorType: MajorType): ByteArray {
        return if (n <= UInt.MAX_VALUE.toULong()) {
            encode(n, majorType)
        } else {
            byteArrayOf(
                (0x1bu.toUByte() or majorType.typeBits()).toByte(),
                (n shr 56).toByte(),
                (n shr 48).toByte(),
                (n shr 40).toByte(),
                (n shr 32).toByte(),
                (n shr 24).toByte(),
                (n shr 16).toByte(),
                (n shr 8).toByte(),
                n.toByte()
            )
        }
    }

    /** Encode as full-width integer (no shortest-form reduction). */
    fun encodeFixedInt64(n: ULong, majorType: MajorType): ByteArray {
        return byteArrayOf(
            (0x1bu.toUByte() or majorType.typeBits()).toByte(),
            (n shr 56).toByte(),
            (n shr 48).toByte(),
            (n shr 40).toByte(),
            (n shr 32).toByte(),
            (n shr 24).toByte(),
            (n shr 16).toByte(),
            (n shr 8).toByte(),
            n.toByte()
        )
    }

    fun encodeFixedInt32(n: UInt, majorType: MajorType): ByteArray {
        val v = n.toLong()
        return byteArrayOf(
            (0x1au.toUByte() or majorType.typeBits()).toByte(),
            (v shr 24).toByte(),
            (v shr 16).toByte(),
            (v shr 8).toByte(),
            v.toByte()
        )
    }

    fun encodeFixedInt16(n: UShort, majorType: MajorType): ByteArray {
        val v = n.toInt()
        return byteArrayOf(
            (0x19u.toUByte() or majorType.typeBits()).toByte(),
            (v shr 8).toByte(),
            v.toByte()
        )
    }
}

private fun ULong.toByte(): Byte = this.toInt().toByte()
private infix fun UByte.or(other: UByte): UByte = (this.toInt() or other.toInt()).toUByte()
