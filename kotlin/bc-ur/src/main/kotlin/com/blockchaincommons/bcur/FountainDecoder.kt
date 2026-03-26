package com.blockchaincommons.bcur

/** Fountain decoder that receives parts and reconstructs the original message. */
internal class FountainDecoder {
    private val decoded = mutableMapOf<Int, FountainPart>()
    private val received = mutableSetOf<List<Int>>()
    private val buffer = mutableMapOf<List<Int>, FountainPart>()
    private val queue = ArrayDeque<Pair<Int, FountainPart>>()
    private var sequenceCount = 0
    private var messageLength = 0
    private var checksum: UInt = 0u
    private var fragmentLength = 0

    /** Returns whether the decoder has received all fragments. */
    val isComplete: Boolean
        get() = messageLength != 0 && decoded.size == sequenceCount

    /** Number of fragments that have been fully decoded. */
    val decodedCount: Int get() = decoded.size

    /** Total number of fragments needed. Returns 0 before the first part is received. */
    val expectedCount: Int get() = sequenceCount

    /** Set of fragment indexes that have been fully decoded. */
    val decodedIndexes: Set<Int> get() = decoded.keys

    /**
     * Partial progress credit from buffered mixed-degree parts.
     *
     * Each buffered part with reduced degree d contributes 1/d,
     * reflecting that it will deliver one full decoded fragment
     * once d-1 of its unknowns are resolved from other sources.
     */
    val bufferContribution: Double
        get() = buffer.keys.sumOf { 1.0 / it.size }

    /** Validates a part against previously received metadata. */
    fun validate(part: FountainPart): Boolean {
        if (received.isEmpty()) return false
        return part.sequenceCount == sequenceCount &&
            part.messageLength == messageLength &&
            part.checksum == checksum &&
            part.data.size == fragmentLength
    }

    /**
     * Receives a fountain part.
     *
     * @return true if the part was new and useful, false if duplicate or decoder complete
     * @throws URException.DecoderError if the part is invalid or inconsistent
     */
    fun receive(part: FountainPart): Boolean {
        if (isComplete) return false

        if (part.sequenceCount == 0 || part.data.isEmpty() || part.messageLength == 0) {
            throw URException.DecoderError("expected non-empty part")
        }

        if (received.isEmpty()) {
            sequenceCount = part.sequenceCount
            messageLength = part.messageLength
            checksum = part.checksum
            fragmentLength = part.data.size
        } else if (!validate(part)) {
            throw URException.DecoderError("part is inconsistent with previous ones")
        }

        val indexes = part.indexes
        if (!received.add(indexes)) {
            return false
        }

        if (part.isSimple) {
            processSimple(part)
        } else {
            processComplex(part)
        }
        return true
    }

    /** Reconstructs the original message if complete, or returns null. */
    fun message(): ByteArray? {
        if (!isComplete) return null

        val combined = ByteArray(sequenceCount * fragmentLength)
        for (idx in 0 until sequenceCount) {
            val part = decoded[idx] ?: throw URException.DecoderError("missing fragment")
            part.data.copyInto(combined, idx * fragmentLength)
        }

        // Validate padding
        for (i in messageLength until combined.size) {
            if (combined[i] != 0.toByte()) {
                throw URException.DecoderError("invalid padding")
            }
        }

        return combined.copyOfRange(0, messageLength)
    }

    private fun processSimple(part: FountainPart) {
        val indexes = part.indexes
        val index = indexes.first()
        decoded[index] = part.deepCopy()
        queue.addLast(index to part)
        processQueue()
    }

    private fun processQueue() {
        while (queue.isNotEmpty()) {
            val (index, simple) = queue.removeLast()

            val toProcess = buffer.keys
                .filter { idxs -> idxs.any { it == index } }
                .toList()

            for (indexes in toProcess) {
                val part = buffer.remove(indexes)!!
                val newIndexes = indexes.toMutableList()
                val pos = newIndexes.indexOf(index)
                newIndexes.removeAt(pos)
                FountainUtils.xorInPlace(part.data, simple.data)

                if (newIndexes.size == 1) {
                    val newIndex = newIndexes.first()
                    decoded[newIndex] = part.deepCopy()
                    queue.addLast(newIndex to part)
                } else {
                    buffer[newIndexes] = part
                }
            }
        }
    }

    private fun processComplex(part: FountainPart) {
        val indexes = part.indexes.toMutableList()
        val toRemove = indexes.filter { it in decoded }

        if (indexes.size == toRemove.size) return

        for (remove in toRemove) {
            val pos = indexes.indexOf(remove)
            indexes.removeAt(pos)
            FountainUtils.xorInPlace(part.data, decoded[remove]!!.data)
        }

        if (indexes.size == 1) {
            val idx = indexes.first()
            decoded[idx] = part.deepCopy()
            queue.addLast(idx to part)
        } else {
            buffer[indexes.toList()] = part
        }
    }
}
