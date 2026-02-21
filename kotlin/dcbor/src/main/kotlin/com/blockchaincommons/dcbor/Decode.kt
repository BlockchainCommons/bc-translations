package com.blockchaincommons.dcbor

/**
 * CBOR decoder implementing dCBOR deterministic encoding validation.
 */
internal object Decode {
    fun decodeCbor(data: ByteArray): Cbor {
        val (cbor, len) = decodeCborInternal(data, 0)
        val remaining = data.size - len
        if (remaining > 0) throw CborException.UnusedData(remaining)
        return cbor
    }

    private fun parseHeader(header: Int): Pair<MajorType, Int> {
        val majorType = when (header shr 5) {
            0 -> MajorType.Unsigned
            1 -> MajorType.Negative
            2 -> MajorType.ByteString
            3 -> MajorType.Text
            4 -> MajorType.Array
            5 -> MajorType.Map
            6 -> MajorType.Tagged
            7 -> MajorType.Simple
            else -> error("impossible")
        }
        val headerValue = header and 31
        return majorType to headerValue
    }

    /**
     * Parse header + varint, returning (majorType, value, totalBytesConsumed).
     */
    private fun parseHeaderVarint(data: ByteArray, offset: Int): Triple<MajorType, ULong, Int> {
        if (offset >= data.size) throw CborException.Underrun()
        val header = data[offset].toInt() and 0xFF
        val (majorType, headerValue) = parseHeader(header)
        val dataRemaining = data.size - offset - 1

        val (value, varintLen) = when {
            headerValue <= 23 -> headerValue.toULong() to 1
            headerValue == 24 -> {
                if (dataRemaining < 1) throw CborException.Underrun()
                val v = (data[offset + 1].toInt() and 0xFF).toULong()
                if (v < 24u) throw CborException.NonCanonicalNumeric()
                v to 2
            }
            headerValue == 25 -> {
                if (dataRemaining < 2) throw CborException.Underrun()
                val v = ((data[offset + 1].toInt() and 0xFF).toULong() shl 8) or
                        (data[offset + 2].toInt() and 0xFF).toULong()
                if (v <= UByte.MAX_VALUE.toULong() && header != 0xF9) throw CborException.NonCanonicalNumeric()
                v to 3
            }
            headerValue == 26 -> {
                if (dataRemaining < 4) throw CborException.Underrun()
                val v = ((data[offset + 1].toInt() and 0xFF).toULong() shl 24) or
                        ((data[offset + 2].toInt() and 0xFF).toULong() shl 16) or
                        ((data[offset + 3].toInt() and 0xFF).toULong() shl 8) or
                        (data[offset + 4].toInt() and 0xFF).toULong()
                if (v <= UShort.MAX_VALUE.toULong() && header != 0xFA) throw CborException.NonCanonicalNumeric()
                v to 5
            }
            headerValue == 27 -> {
                if (dataRemaining < 8) throw CborException.Underrun()
                val v = ((data[offset + 1].toInt() and 0xFF).toULong() shl 56) or
                        ((data[offset + 2].toInt() and 0xFF).toULong() shl 48) or
                        ((data[offset + 3].toInt() and 0xFF).toULong() shl 40) or
                        ((data[offset + 4].toInt() and 0xFF).toULong() shl 32) or
                        ((data[offset + 5].toInt() and 0xFF).toULong() shl 24) or
                        ((data[offset + 6].toInt() and 0xFF).toULong() shl 16) or
                        ((data[offset + 7].toInt() and 0xFF).toULong() shl 8) or
                        (data[offset + 8].toInt() and 0xFF).toULong()
                if (v <= UInt.MAX_VALUE.toULong() && header != 0xFB) throw CborException.NonCanonicalNumeric()
                v to 9
            }
            else -> throw CborException.UnsupportedHeaderValue(headerValue.toUByte())
        }
        return Triple(majorType, value, varintLen)
    }

    /**
     * Decode one CBOR item starting at [offset].
     * Returns (Cbor, endOffset) where endOffset is the first byte after the item.
     */
    private fun decodeCborInternal(data: ByteArray, offset: Int): Pair<Cbor, Int> {
        if (offset >= data.size) throw CborException.Underrun()
        val (majorType, value, headerVarintLen) = parseHeaderVarint(data, offset)
        val pos = offset + headerVarintLen

        return when (majorType) {
            MajorType.Unsigned -> Cbor(CborCase.Unsigned(value)) to pos
            MajorType.Negative -> Cbor(CborCase.Negative(value)) to pos
            MajorType.ByteString -> {
                val dataLen = value.toInt()
                if (pos + dataLen > data.size) throw CborException.Underrun()
                val bytes = data.copyOfRange(pos, pos + dataLen)
                Cbor(CborCase.CborByteString(ByteString(bytes))) to (pos + dataLen)
            }
            MajorType.Text -> {
                val dataLen = value.toInt()
                if (pos + dataLen > data.size) throw CborException.Underrun()
                val bytes = data.copyOfRange(pos, pos + dataLen)
                val string = try {
                    String(bytes, Charsets.UTF_8)
                } catch (e: Exception) {
                    throw CborException.InvalidString(e.message ?: "UTF-8 error")
                }
                if (!StringUtil.isNfc(string)) throw CborException.NonCanonicalString()
                Cbor(CborCase.Text(string)) to (pos + dataLen)
            }
            MajorType.Array -> {
                var p = pos
                val items = mutableListOf<Cbor>()
                for (i in 0 until value.toInt()) {
                    val (item, nextPos) = decodeCborInternal(data, p)
                    items.add(item)
                    p = nextPos
                }
                Cbor(CborCase.Array(items)) to p
            }
            MajorType.Map -> {
                var p = pos
                val map = CborMap()
                for (i in 0 until value.toInt()) {
                    val (key, keyEnd) = decodeCborInternal(data, p)
                    p = keyEnd
                    val (v, valEnd) = decodeCborInternal(data, p)
                    p = valEnd
                    map.insertNext(key, v)
                }
                Cbor(CborCase.CborMap(map)) to p
            }
            MajorType.Tagged -> {
                val (item, end) = decodeCborInternal(data, pos)
                Cbor.taggedValue(value, item) to end
            }
            MajorType.Simple -> when (headerVarintLen) {
                3 -> {
                    // f16
                    val bits = value.toUShort()
                    FloatCodec.validateCanonicalF16(bits)
                    val f = FloatCodec.f16FromBits(bits)
                    Cbor.fromFloat(f) to pos
                }
                5 -> {
                    // f32
                    val bits = value.toUInt()
                    val f = java.lang.Float.intBitsToFloat(bits.toInt())
                    FloatCodec.validateCanonicalF32(f)
                    Cbor.fromFloat(f) to pos
                }
                9 -> {
                    // f64
                    val f = java.lang.Double.longBitsToDouble(value.toLong())
                    FloatCodec.validateCanonicalF64(f)
                    Cbor.fromDouble(f) to pos
                }
                else -> when (value.toInt()) {
                    20 -> Cbor.`false`() to pos
                    21 -> Cbor.`true`() to pos
                    22 -> Cbor.`null`() to pos
                    else -> throw CborException.InvalidSimpleValue()
                }
            }
        }
    }
}
