package com.blockchaincommons.bcrand

/**
 * Abstract base for random number generators.
 *
 * Concrete subclasses must implement [nextU32] and [nextU64].
 * Default implementations of [randomData] and [fillRandomData] use
 * little-endian packing of [nextU64] values.
 */
abstract class RandomNumberGenerator {

    abstract fun nextU32(): UInt

    abstract fun nextU64(): ULong

    open fun randomData(size: Int): ByteArray {
        val data = ByteArray(size)
        fillRandomData(data)
        return data
    }

    open fun fillRandomData(data: ByteArray) {
        var i = 0
        while (i + 8 <= data.size) {
            val v = nextU64()
            for (j in 0 until 8) {
                data[i + j] = (v shr (j * 8)).toByte()
            }
            i += 8
        }
        if (i < data.size) {
            val v = nextU64()
            for (j in 0 until data.size - i) {
                data[i + j] = (v shr (j * 8)).toByte()
            }
        }
    }
}
