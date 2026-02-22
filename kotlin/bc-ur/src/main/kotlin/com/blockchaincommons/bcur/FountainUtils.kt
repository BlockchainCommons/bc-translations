package com.blockchaincommons.bcur

/** Utility functions for fountain encoding. */
internal object FountainUtils {
    /** Calculates the actual fragment length given a data length and max fragment length. */
    fun fragmentLength(dataLength: Int, maxFragmentLength: Int): Int {
        val fragmentCount = divCeil(dataLength, maxFragmentLength)
        return divCeil(dataLength, fragmentCount)
    }

    /**
     * Splits data into fragments of equal length, padding with zeros if needed.
     */
    fun partition(data: ByteArray, fragmentLength: Int): List<ByteArray> {
        val padding = (fragmentLength - (data.size % fragmentLength)) % fragmentLength
        val padded = data + ByteArray(padding)
        return padded.toList().chunked(fragmentLength).map { it.toByteArray() }
    }

    /**
     * Selects fragment indexes for a given sequence number.
     *
     * For simple parts (sequence <= fragmentCount), returns a single index.
     * For mixed parts, uses Xoshiro256 to randomly select and combine fragments.
     */
    fun chooseFragments(sequence: Int, fragmentCount: Int, checksum: UInt): List<Int> {
        if (sequence <= fragmentCount) {
            return listOf(sequence - 1)
        }

        val seed = sequence.toUInt().toBytesBigEndian() + checksum.toBytesBigEndian()

        val xoshiro = Xoshiro256.fromBytes(seed)
        val degree = xoshiro.chooseDegree(fragmentCount)
        val indexes = (0 until fragmentCount).toMutableList()
        val shuffled = xoshiro.shuffled(indexes).toMutableList()
        return shuffled.subList(0, degree).toList()
    }

    /** XORs [other] into [target] in place, mutating [target]. */
    fun xorInPlace(target: ByteArray, other: ByteArray) {
        for (i in target.indices) {
            target[i] = (target[i].toInt() xor other[i].toInt()).toByte()
        }
    }

    private fun divCeil(a: Int, b: Int): Int {
        val d = a / b
        val r = a % b
        return if (r > 0) d + 1 else d
    }
}
