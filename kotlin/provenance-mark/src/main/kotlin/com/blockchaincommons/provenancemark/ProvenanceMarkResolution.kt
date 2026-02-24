package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate

enum class ProvenanceMarkResolution(val code: Int) {
    Low(0),
    Medium(1),
    Quartile(2),
    High(3);

    fun linkLength(): Int = when (this) {
        Low -> 4
        Medium -> 8
        Quartile -> 16
        High -> 32
    }

    fun seqBytesLength(): Int = when (this) {
        Low -> 2
        Medium, Quartile, High -> 4
    }

    fun dateBytesLength(): Int = when (this) {
        Low -> 2
        Medium -> 4
        Quartile, High -> 6
    }

    fun fixedLength(): Int = linkLength() * 3 + seqBytesLength() + dateBytesLength()

    fun keyRange(): IntRange = 0 until linkLength()

    fun chainIdRange(): IntRange = 0 until linkLength()

    fun hashRange(): IntRange {
        val start = chainIdRange().last + 1
        return start until (start + linkLength())
    }

    fun seqBytesRange(): IntRange {
        val start = hashRange().last + 1
        return start until (start + seqBytesLength())
    }

    fun dateBytesRange(): IntRange {
        val start = seqBytesRange().last + 1
        return start until (start + dateBytesLength())
    }

    fun infoRangeStart(): Int = dateBytesRange().last + 1

    fun serializeDate(date: CborDate): ByteArray = when (this) {
        Low -> date.serialize2Bytes()
        Medium -> date.serialize4Bytes()
        Quartile, High -> date.serialize6Bytes()
    }

    fun deserializeDate(data: ByteArray): CborDate = when {
        this == Low && data.size == 2 -> deserialize2Bytes(data)
        this == Medium && data.size == 4 -> deserialize4Bytes(data)
        (this == Quartile || this == High) && data.size == 6 -> deserialize6Bytes(data)
        else -> throw ProvenanceMarkException.ResolutionError(
            "invalid date length: expected 2, 4, or 6 bytes, got ${data.size}"
        )
    }

    fun serializeSeq(seq: UInt): ByteArray = when (seqBytesLength()) {
        2 -> {
            if (seq > UShort.MAX_VALUE.toUInt()) {
                throw ProvenanceMarkException.ResolutionError(
                    "sequence number $seq out of range for 2-byte format (max ${UShort.MAX_VALUE})"
                )
            }
            byteArrayOf(((seq shr 8) and 0xFFu).toByte(), (seq and 0xFFu).toByte())
        }
        4 -> byteArrayOf(
            ((seq shr 24) and 0xFFu).toByte(),
            ((seq shr 16) and 0xFFu).toByte(),
            ((seq shr 8) and 0xFFu).toByte(),
            (seq and 0xFFu).toByte(),
        )
        else -> throw IllegalStateException("unsupported sequence byte length")
    }

    fun deserializeSeq(data: ByteArray): UInt = when (seqBytesLength()) {
        2 -> {
            if (data.size != 2) {
                throw ProvenanceMarkException.ResolutionError(
                    "invalid sequence number length: expected 2 or 4 bytes, got ${data.size}"
                )
            }
            (((data[0].toUInt() and 0xFFu) shl 8) or (data[1].toUInt() and 0xFFu))
        }
        4 -> {
            if (data.size != 4) {
                throw ProvenanceMarkException.ResolutionError(
                    "invalid sequence number length: expected 2 or 4 bytes, got ${data.size}"
                )
            }
            ((data[0].toUInt() and 0xFFu) shl 24) or
                ((data[1].toUInt() and 0xFFu) shl 16) or
                ((data[2].toUInt() and 0xFFu) shl 8) or
                (data[3].toUInt() and 0xFFu)
        }
        else -> throw IllegalStateException("unsupported sequence byte length")
    }

    fun toCbor(): Cbor = Cbor.fromInt(code)

    override fun toString(): String = when (this) {
        Low -> "low"
        Medium -> "medium"
        Quartile -> "quartile"
        High -> "high"
    }

    companion object {
        fun fromCode(value: Int): ProvenanceMarkResolution = when (value) {
            0 -> Low
            1 -> Medium
            2 -> Quartile
            3 -> High
            else -> throw ProvenanceMarkException.ResolutionError(
                "invalid provenance mark resolution value: $value"
            )
        }

        fun fromCbor(cbor: Cbor): ProvenanceMarkResolution {
            return fromCode(cbor.tryInt())
        }
    }
}
