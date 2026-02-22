package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.Cbor

/** A part emitted by a fountain encoder. */
internal class FountainPart(
    val sequence: Int,
    val sequenceCount: Int,
    val messageLength: Int,
    val checksum: UInt,
    val data: ByteArray
) {
    /** The indexes of message segments combined in this part. */
    val indexes: List<Int>
        get() = FountainUtils.chooseFragments(sequence, sequenceCount, checksum)

    /** Whether this part represents a single original segment. */
    val isSimple: Boolean
        get() = indexes.size == 1

    /** The sequence identifier string (e.g. "1-9"). */
    val sequenceId: String
        get() = "$sequence-$sequenceCount"

    /** Encodes this part as a CBOR byte array. */
    fun toCbor(): ByteArray {
        val cbor = Cbor.fromArray(listOf(
            Cbor.fromInt(sequence),
            Cbor.fromInt(sequenceCount),
            Cbor.fromInt(messageLength),
            Cbor.fromUInt(checksum),
            Cbor.fromByteString(data)
        ))
        return cbor.toCborData()
    }

    /** Creates a copy with an independent data array. */
    fun deepCopy(): FountainPart =
        FountainPart(sequence, sequenceCount, messageLength, checksum, data.copyOf())

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is FountainPart) return false
        return sequence == other.sequence &&
            sequenceCount == other.sequenceCount &&
            messageLength == other.messageLength &&
            checksum == other.checksum &&
            data.contentEquals(other.data)
    }

    override fun hashCode(): Int {
        var result = sequence
        result = 31 * result + sequenceCount
        result = 31 * result + messageLength
        result = 31 * result + checksum.hashCode()
        result = 31 * result + data.contentHashCode()
        return result
    }

    companion object {
        /** Decodes a fountain part from a CBOR byte array. */
        fun fromCbor(data: ByteArray): FountainPart {
            val cbor = Cbor.tryFromData(data)
            val array = cbor.tryArray()
            if (array.size != 5) {
                throw URException.DecoderError("invalid CBOR array length")
            }
            return FountainPart(
                sequence = array[0].tryInt(),
                sequenceCount = array[1].tryInt(),
                messageLength = array[2].tryInt(),
                checksum = array[3].tryUInt(),
                data = array[4].tryByteStringData()
            )
        }
    }
}
