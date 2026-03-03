package com.blockchaincommons.bcur

/** Multipart UR encoder using fountain codes. */
class MultipartEncoder(ur: UR, maxFragmentLen: Int) {
    private val encoder: FountainEncoder
    private val urType: String = ur.urTypeStr
    private val messageData: ByteArray

    init {
        val data = ur.cbor.toCborData()
        messageData = data
        encoder = FountainEncoder(data, maxFragmentLen)
    }

    /** Emits the next UR part string. Single-part URs use the simple
     *  `ur:type/payload` format; multi-part URs use `ur:type/seq-total/payload`. */
    fun nextPart(): String {
        val part = encoder.nextPart()
        if (partCount == 1) {
            return UREncoding.encode(messageData, urType)
        }
        val body = Bytewords.encode(part.toCbor(), BytewordsStyle.Minimal)
        return "ur:$urType/${part.sequenceId}/$body"
    }

    /** The current count of emitted parts. */
    val currentIndex: Int get() = encoder.currentSequence

    /** The number of fragments the message was split into. */
    val partCount: Int get() = encoder.fragmentCount

    /** The fragment indexes included in the most recently emitted part. */
    val lastFragmentIndexes: List<Int> get() = encoder.lastFragmentIndexes
}
