package com.blockchaincommons.bcur

/** Multipart UR encoder using fountain codes. */
class MultipartEncoder(ur: UR, maxFragmentLen: Int) {
    private val encoder: FountainEncoder
    private val urType: String = ur.urTypeStr

    init {
        val data = ur.cbor.toCborData()
        encoder = FountainEncoder(data, maxFragmentLen)
    }

    /** Emits the next UR part string. */
    fun nextPart(): String {
        val part = encoder.nextPart()
        val body = Bytewords.encode(part.toCbor(), BytewordsStyle.Minimal)
        return "ur:$urType/${part.sequenceId}/$body"
    }

    /** The current count of emitted parts. */
    val currentIndex: Int get() = encoder.currentSequence

    /** The number of fragments the message was split into. */
    val partCount: Int get() = encoder.fragmentCount
}
