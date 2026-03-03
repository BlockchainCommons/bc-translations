package com.blockchaincommons.bcur

/** Fountain encoder that emits an unbounded stream of parts. */
internal class FountainEncoder(message: ByteArray, maxFragmentLength: Int) {
    private val parts: List<ByteArray>
    private val messageLength: Int = message.size
    private val checksum: UInt = Crc32.checksum(message)
    var currentSequence: Int = 0
        private set
    var lastFragmentIndexes: List<Int> = emptyList()
        private set

    init {
        require(message.isNotEmpty()) { "expected non-empty message" }
        require(maxFragmentLength > 0) { "expected positive maximum fragment length" }
        val fragLen = FountainUtils.fragmentLength(message.size, maxFragmentLength)
        parts = FountainUtils.partition(message, fragLen)
    }

    /** Returns the number of fragments. */
    val fragmentCount: Int get() = parts.size

    /** Returns whether all original segments have been emitted at least once. */
    val isComplete: Boolean get() = currentSequence >= parts.size

    /** Emits the next fountain part. */
    fun nextPart(): FountainPart {
        currentSequence++
        val indexes = FountainUtils.chooseFragments(currentSequence, parts.size, checksum)
        lastFragmentIndexes = indexes
        val mixed = ByteArray(parts[0].size)
        for (index in indexes) {
            FountainUtils.xorInPlace(mixed, parts[index])
        }
        return FountainPart(
            sequence = currentSequence,
            sequenceCount = parts.size,
            messageLength = messageLength,
            checksum = checksum,
            data = mixed
        )
    }
}
